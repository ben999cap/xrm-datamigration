﻿using System.Net;
using System.Threading;
using Capgemini.DataMigration.Core;
using Capgemini.DataMigration.Resiliency.Polly;
using Capgemini.Xrm.DataMigration.CrmStore.Config;
using Capgemini.Xrm.DataMigration.Engine;
using Capgemini.Xrm.DataMigration.Repositories;
using CommandLine;
using Microsoft.Xrm.Tooling.Connector;

namespace Capgemini.Xrm.DataMigration.Cli
{
    internal static class Program
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Parser.Default
               .ParseArguments<ExportOptions, ImportOptions>(args)
               .MapResult(
               (ExportOptions opts) => ExportData(opts),
               (ImportOptions opts) => ImportData(opts),
               (err) => 1);
        }

        private static int ExportData(ExportOptions options)
        {
            using (var serviceClient = new CrmServiceClient(options.ConnectionString))
            {
                var config = CrmExporterConfig.GetConfiguration(options.ConfigurationFile);
                new CrmFileDataExporter(
                    Logger,
                    new EntityRepository(serviceClient.OrganizationServiceProxy, new ServiceRetryExecutor(), config.EnableLookupCaching ? RepositoryCachingMode.Lookup : RepositoryCachingMode.None),
                    config,
                    CancellationToken.None).MigrateData();
                return 0;
            }
        }

        private static int ImportData(ImportOptions options)
        {
            using (var serviceClient = new CrmServiceClient(options.ConnectionString))
            {
                var config = CrmImportConfig.GetConfiguration(options.ConfigurationFile);

                new CrmFileDataImporter(
                    Logger,
                    new EntityRepository(serviceClient, new ServiceRetryExecutor(), config.EnableLookupCaching ? RepositoryCachingMode.Lookup : RepositoryCachingMode.None),
                    config,
                    CancellationToken.None).MigrateData();
                return 0;
            }
        }
    }
}