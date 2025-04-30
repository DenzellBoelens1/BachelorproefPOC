using System;
using NBomber.CSharp;
using NBomber.Contracts;
using Webshop.Backend.LoadTests.priceTest;

namespace Webshop.Backend.LoadTests
{
    class LoadTestsProgram
    {
        static void Main(string[] args)
        {
            // 1) Stock update scenarios
            var restStock = RestStockUpdate.CreateScenario();
            var gqlStock = GraphQLStockUpdate.CreateScenario();
            var signalRStock = SignalRStockUpdate.CreateScenario();
            var wsStock = WebSocketStockUpdate.CreateScenario();

            // 2) Complexity scenarios
            var restSimple = RestConfigComplexity.CreateScenario(0, false);
            var restMedium = RestConfigComplexity.CreateScenario(5, true);
            var restComplex = RestConfigComplexity.CreateScenario(20, true);

            var gqlSimple = GraphQLConfigComplexity.CreateScenario(0, false);
            var gqlMedium = GraphQLConfigComplexity.CreateScenario(5, true);
            var gqlComplex = GraphQLConfigComplexity.CreateScenario(20, true);

            var wsScenario = WebSocketConfigComplexity.CreateScenario(0, false);
            var signalrScenario = SignalRConfigComplexity.CreateScenario(0, false);


            // Run sequentially
            //NBomberRunner.RegisterScenarios(restStock).Run();
            //NBomberRunner.RegisterScenarios(gqlStock).Run();
            //NBomberRunner.RegisterScenarios(signalRStock).Run();
            //NBomberRunner.RegisterScenarios(wsStock).Run();

            //NBomberRunner.RegisterScenarios(restSimple).Run();
            //NBomberRunner.RegisterScenarios(restMedium).Run();
            //NBomberRunner.RegisterScenarios(restComplex).Run();

            //NBomberRunner.RegisterScenarios(gqlSimple).Run();
            //NBomberRunner.RegisterScenarios(gqlMedium).Run();
            //NBomberRunner.RegisterScenarios(gqlComplex).Run();

            //NBomberRunner.RegisterScenarios(wsScenario).Run();
            //NBomberRunner.RegisterScenarios(signalrScenario).Run();

            NBomberRunner
                .RegisterScenarios(
                    PriceLoadTestScenarios.RestPriceScenario(),
                    PriceLoadTestScenarios.GraphQLPriceScenario(),
                    PriceLoadTestScenarios.SignalRPriceScenario(),
                    PriceLoadTestScenarios.WebSocketPriceScenario()
                )
                .Run();
        }
    }
}