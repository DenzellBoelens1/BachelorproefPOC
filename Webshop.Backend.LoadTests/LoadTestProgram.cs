using System;
using NBomber.CSharp;
using NBomber.Contracts;
using Webshop.Backend.LoadTests.priceTest; // or wherever your scenarios live

namespace Webshop.Backend.LoadTests
{
    class LoadTestsProgram
    {
        static void Main(string[] args)
        {
            //// --- 1) STOCK UPDATE SCENARIOS ---
            //var restStock = RestStockUpdate.CreateScenario();
            //var gqlStock = GraphQLStockUpdate.CreateScenario();
            //var signalRStock = SignalRStockUpdate.CreateScenario();
            //var wsStock = WebSocketStockUpdate.CreateScenario();

            //NBomberRunner
            //    .RegisterScenarios(restStock, gqlStock, signalRStock, wsStock)
            //    .WithReportFileName("stock_update_report")
            //    .WithReportFolder("./reports/stock")
            //    .Run();

            //// --- 2) PRICE CALCULATION SCENARIOS ---
            //var restPrice = PriceLoadTestScenarios.RestPriceScenario();
            //var gqlPrice = PriceLoadTestScenarios.GraphQLPriceScenario();
            //var signalRPrice = PriceLoadTestScenarios.SignalRPriceScenario();
            //var wsPrice = PriceLoadTestScenarios.WebSocketPriceScenario();

            //NBomberRunner
            //    .RegisterScenarios(restPrice, gqlPrice, signalRPrice, wsPrice)
            //    .WithReportFileName("price_calculation_report")
            //    .WithReportFolder("./reports/price")
            //    .Run();

            //// --- 3) CONFIGURATION COMPLEXITY SCENARIOS ---
            //var restSimple = RestConfigComplexity.CreateScenario(0, false);
            //var restMedium = RestConfigComplexity.CreateScenario(5, true);
            //var restComplex = RestConfigComplexity.CreateScenario(20, true);

            //var gqlSimple = GraphQLConfigComplexity.CreateScenario(0, false);
            //var gqlMedium = GraphQLConfigComplexity.CreateScenario(5, true);
            //var gqlComplex = GraphQLConfigComplexity.CreateScenario(20, true);

            //var wsConfig = WebSocketConfigComplexity.CreateScenario(0, false);
            //var signalRConfig = SignalRConfigComplexity.CreateScenario(0, false);

            //NBomberRunner
            //    .RegisterScenarios(
            //        restSimple,
            //        restMedium,
            //        restComplex,
            //        gqlSimple,
            //        gqlMedium,
            //        gqlComplex,
            //        wsConfig,
            //        signalRConfig
            //    )
            //    .WithReportFileName("config_complexity_report")
            //    .WithReportFolder("./reports/config")
            //    .Run();


            //// --- 4) TRANSPORT COMPARISON SCENARIOS ---
            //var wsTransportScenario = SignalRTransportComparison.CreateWebSocketScenario();
            //var sseTransportScenario = SignalRTransportComparison.CreateSseScenario();

            //NBomberRunner
            //    .RegisterScenarios(wsTransportScenario, sseTransportScenario)
            //    .WithReportFileName("transport_comparison_report")
            //    .WithReportFolder("reports/transport")
            //    .Run();

            //// --- 5) GRAPHQL INLINE vs PERSISTED QUERY SCENARIOS ---
            //var inlineGqlScenario = GraphQLPersistedQueryScenario.CreateInlineQueryScenario();

            //NBomberRunner
            //    .RegisterScenarios(
            //        inlineGqlScenario

            //    )
            //    .WithReportFileName("graphql_query_report")
            //    .WithReportFolder("reports/graphql")
            //    .Run();


            // --- 6) MIXED WORKLOAD SCENARIOS ---
            var readScenario = MixedWorkloadScenario.CreateReadScenario();
            var writeScenario = MixedWorkloadScenario.CreateWriteScenario();

            NBomberRunner
                .RegisterScenarios(readScenario, writeScenario)
                .WithReportFileName("mixed_workload_report")
                .WithReportFolder("./reports/mixed")
                .Run();
        }
    }
}
