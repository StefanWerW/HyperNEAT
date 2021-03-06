﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;

using HyperNeatLib.Factories;
using HyperNeatLib.Helpers;
using HyperNeatLib.Interfaces;

using Newtonsoft.Json;

namespace HyperNeat.Bitcoin
{
    public class Calc
    {
        public event EventHandler ProgressUpdate;

        public bool DoSerialize { get; set; } = false;

        public bool Stop { get; set; } = false;

        public long TradeCount { get; set; }

        public const int TradePeriod = 5;

        public const int TradeMonths = 24;

        public const double MinTrade = 0.008;

        public const int SignalLength = 30;

        public const double SignalWeight = 0.7;

        public int TradeLength
        {
            get
            {
                return (TradeMonths * 30 * 24 * 60 / TradePeriod);
            }
        }

        public void StartTrading(object input)
        {
            GenerationIdSingleton.Instance.Reset();

            var population = PopulationFactory.CreatePopulation(13, 3, 200);

            population.InitialSpeciesSize = 10;

            if (!Directory.Exists("old_nets"))
            {
                Directory.CreateDirectory("old_nets");
            }

            foreach (var file in Directory.GetFiles("old_nets"))
            {
                File.Delete(file);
            }

            RunTrading(population);
        }

        public void RunTrading(IPopulation population)
        {
            //var lines = File.ReadAllLines("trades.txt");

            if (File.Exists("PopulationDevelopment.txt"))
            {
                File.Delete("PopulationDevelopment.txt");
            }

            var random = new Random();

            //var tradeHistory = lines.Select(
            //    l =>
            //    {
            //        var split = l.Split(';');
            //        return new Tuple<DateTime, double, double>(
            //            DateTime.Parse(split[1]),
            //            double.Parse(split[0]),
            //            double.Parse(split[2]));
            //    })
            //    .OrderBy(t => t.Item1)
            //    .GroupBy(t => new DateTime(t.Item1.Year, t.Item1.Month, t.Item1.Day, t.Item1.Hour, (t.Item1.Minute / 5) * 5, 0))
            //    .Select(
            //        g =>
            //        Tuple.Create(
            //            g.Key,
            //            g.First().Item3,
            //            g.Last().Item3,
            //            g.Min(t => t.Item3),
            //            g.Max(t => t.Item3),
            //            g.Sum(t => t.Item2)))
            //    .ToList();

            //TradeCount = 0;

            //using (StreamReader sr = new StreamReader("btceUSD.csv"))
            //{
            //    String line;
            //    // Read and display lines from the file until the end of 
            //    // the file is reached.
            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        TradeCount++;
            //    }
            //}

            var lockObject = new Object();

            int count = 1;

            int minUnixTime = 1372550400;
            int maxUnixTime = 1453204641;

            var start = random.Next(minUnixTime, (int)(maxUnixTime - (TradeMonths * 30 * 24 * 60 * 60)));
            var end = random.Next(start + ((TradeMonths * 30 * 24 * 60 * 60) / 2), start + (TradeMonths * 30 * 24 * 60 * 60));

            var trades = GetTradeEntries(1372550400, 1440892800, true);

            var testTrades = GetTradeEntries(1440892800, 1453204641, true);

            //while (trades == null || trades.First().FirstPrice < trades.Last().LastPrice)
            //{
            //    trades = null;

            //    GC.Collect();

            //    start = random.Next(minUnixTime, (int)(maxUnixTime - (TradeMonths * 30 * 24 * 60 * 60)));
            //    end = random.Next(start + ((TradeMonths * 30 * 24 * 60 * 60) / 2), start + (TradeMonths * 30 * 24 * 60 * 60));

            //    trades = GetTradeEntries(start, end, false);
            //}

            var ascending = false;

            File.Delete("stats.txt");

            var endWeight = 0.1;

            double bestGenomeOverallFitness = 0.0;
            int lastBestGenomeOverallChange = 1;

            //Calculate exonential moving average start
            var startEma = trades.First().FirstPrice;

            foreach (var tr in trades.Take(10))
            {
                startEma = startEma + 0.0328 * (tr.FirstPrice - startEma);
            }

            trades = trades.Skip(10).ToList();

            var teststartEma = testTrades.First().FirstPrice;

            foreach (var tr in testTrades.Take(10))
            {
                teststartEma = teststartEma + 0.0328 * (tr.FirstPrice - teststartEma);
            }

            testTrades = testTrades.Skip(10).ToList();

            List<TradeEntry> newTrades = null;

            //var thread = new Thread(
            //    () =>
            //    {
            //        while (newTrades == null || newTrades.Count < 8000
            //               || (ascending && newTrades.First().FirstPrice <= newTrades.Last().LastPrice)
            //               || (!ascending && newTrades.First().FirstPrice > newTrades.Last().LastPrice))
            //        {
            //            newTrades = null;

            //            GC.Collect();

            //            start = random.Next(minUnixTime, (int)(maxUnixTime - (TradeMonths * 30 * 24 * 60 * 60)));
            //            end = random.Next(
            //                start + ((TradeMonths * 30 * 24 * 60 * 60) / 2),
            //                start + (TradeMonths * 30 * 24 * 60 * 60));

            //            newTrades = GetTradeEntries(start, end, !ascending);
            //        }

            //        ascending = !ascending;
            //    });

            //thread.Start();

            while (true)
            {
                //if (count - lastBestGenomeOverallChange > 20)
                //{
                //    thread.Join();

                //    startEma = newTrades.First().FirstPrice;

                //    foreach (var tr in newTrades.Take(10))
                //    {
                //        startEma = startEma + 0.18 * (tr.FirstPrice - startEma);
                //    }

                //    newTrades = newTrades.Skip(10).ToList();

                //    trades = newTrades;

                //    bestGenomeOverallFitness = 0.0;
                //    lastBestGenomeOverallChange = count;

                //    thread = new Thread(
                //        () =>
                //        {
                //            while (newTrades == null || newTrades.Count < 8000
                //                   || (ascending && newTrades.First().FirstPrice <= newTrades.Last().LastPrice)
                //                   || (!ascending && newTrades.First().FirstPrice > newTrades.Last().LastPrice))
                //            {
                //                newTrades = null;

                //                GC.Collect();

                //                start = random.Next(minUnixTime, (int)(maxUnixTime - (TradeMonths * 30 * 24 * 60 * 60)));
                //                end = random.Next(
                //                    start + ((TradeMonths * 30 * 24 * 60 * 60) / 2),
                //                    start + (TradeMonths * 30 * 24 * 60 * 60));

                //                newTrades = GetTradeEntries(start, end, !ascending);
                //            }

                //            ascending = !ascending;
                //        });

                //    thread.Start();
                //}

                var turnWeightDifference = (1 - endWeight) / trades.Count;

                var bestGenomeFitness = 0.0;
                var bestGenomeBuys = 0;
                var bestGenomeSells = 0;
                var bestGenomeMoney = 0.0;
                var bestGenomeTradeHistory = string.Empty;
                INetwork bestNetwork = population.Networks.First();

                var startPrice = trades.First().FirstPrice;
                var endPrice = trades.Last().LastPrice;

                var priceDifference = startPrice / endPrice;

                var tradeCount = trades.Count;

                Parallel.ForEach(
                    population.Networks,
                    network =>
                        {
                            this.RunNetworkTrades(
                                trades,
                                startEma,
                                network,
                                tradeCount,
                                lockObject,
                                ref bestGenomeFitness,
                                ref bestGenomeMoney,
                                ref bestGenomeBuys,
                                ref bestGenomeSells,
                                ref bestGenomeTradeHistory,
                                ref bestNetwork);
                        });

                if (bestGenomeFitness / bestGenomeOverallFitness > 1.1)
                {
                    bestGenomeOverallFitness = bestGenomeFitness;
                    lastBestGenomeOverallChange = count;
                }


                File.AppendAllText(
                    "PopulationDevelopment.txt",
                    string.Join(
                        ";",
                        population.Networks.Select(n => n.Fingerprint())
                        .GroupBy(f => f)
                        .Select(g => g.Key + "\t" + g.Count())) + "\r\n");

                if (Stop)
                {
                    return;
                }

                var usde = 500.0;
                var bitcoine = 0.0;
                var buyCounte = 0;
                var sellCounte = 0;
                var netTradeHistorye = string.Empty;
                
                var testGenomeFitness = 0.0;
                var testGenomeBuys = 0;
                var testGenomeSells = 0;
                var testGenomeMoney = 0.0;
                var testGenomeTradeHistory = string.Empty;
                INetwork testNetwork = (INetwork)bestNetwork.Clone();

                this.RunNetworkTrades(
                    testTrades,
                    teststartEma,
                    testNetwork,
                    testTrades.Count(),
                    lockObject,
                    ref testGenomeFitness,
                    ref testGenomeMoney,
                    ref testGenomeBuys,
                    ref testGenomeSells,
                    ref testGenomeTradeHistory,
                    ref testNetwork);

                var bestGenomeTotalMoney = testGenomeMoney * testTrades.First().FirstPrice / testTrades.Last().LastPrice;//usde + bitcoine * tradeHistory.Last().Item4 * 0.998;

                if (ProgressUpdate != null)
                {
                    ProgressUpdate(this, new ProgressEventArgs()
                    {
                        BestGenomeFitness = bestGenomeFitness,
                        BestGenomeBuys = bestGenomeBuys,
                        BestGenomeSells = bestGenomeSells,
                        WeeklyProfit = (Math.Pow(bestGenomeMoney / 2000.0, ((60.0 / TradePeriod) * 24.0 * 30 * TradeMonths) / trades.Count) - 1) * 100,
                        CurrentGeneration = population.CurrentGeneration,
                        BestGenomeMoney = bestGenomeMoney,
                        StartPrice = startPrice,
                        EndPrice = endPrice,
                        SpecieCount = population.Species.Count(s => s.Networks.Any()),
                        AverageFitness = population.Networks.Average(n => n.Fitness),
                        TradeHistory = bestGenomeTradeHistory,
                        NetworkFingerprint = bestNetwork.Fingerprint(),
                        TradeCounts = trades.Count,
                        BestGenomeTotalMoney = bestGenomeTotalMoney,
                        SpeciesOverview = string.Join(
                            "\r\n",
                            population.Species
                                .OrderByDescending(s => s.Networks.Count)
                                .ThenByDescending(s => s.MaxGenomeFitness)
                                .Select(s => s.Networks.Count + ", " + s.MaxGenomeFitness)),
                        StartTime = trades.First().Key,
                        EndTime = trades.Last().Key
                    });
                }

                //Console.WriteLine(
                //    "Gen: {4}, Max Fit: {0:0.00000}, AVG Fit: {9:0.00000}, Money: {5:0.00}, Best Money: {11:0.00}, Buy: {1}, Sells: {2}, WP: {3:0.00000}, Start: {6}, End: {7}, Spec: {8}, Trade History: {10}",
                //    bestGenomeFitness,
                //    bestGenomeBuys,
                //    bestGenomeSells,
                //    (Math.Pow(bestGenomeMoney / 2000.0, ((60.0 / TradePeriod) * 24.0 * 30 * TradeMonths) / trades.Count) - 1) * 100,
                //    population.CurrentGeneration,
                //    bestGenomeMoney,
                //    startPrice,
                //    endPrice,
                //    population.Species.Count(s => s.Networks.Any()),
                //    population.Networks.Average(n => n.Fitness),
                //    bestGenomeTradeHistory,
                //    bestGenomeTotalMoney
                //    );

                File.AppendAllText(
                    "stats.txt",
                    string.Format(
                        "Time: {11:yyyy-MM-dd HH:mm:ss}, Gen: {4}, Max Fit: {0:0.00000}, AVG Fit: {9:0.00000}, Money: {5:0.00}, Best Money: {14:0.00}, Buy: {1}, Sells: {2}, YP: {3:0.00000}, Start: {6}, End: {7}, Count: {13}, Spec: {8}, FP:{12}, Trade History: {10} \n",
                        bestGenomeFitness,
                        bestGenomeBuys,
                        bestGenomeSells,
                        (Math.Pow(bestGenomeMoney / 2000.0, ((60.0 / TradePeriod) * 24.0 * 30 * TradeMonths) / trades.Count) - 1) * 100,
                        population.CurrentGeneration,
                        bestGenomeMoney,
                        startPrice,
                        endPrice,
                        population.Species.Count(s => s.Networks.Any()),
                        population.Networks.Average(n => n.Fitness),
                        bestGenomeTradeHistory,
                        DateTime.Now,
                        bestNetwork.Fingerprint(),
                        trades.Count,
                        bestGenomeTotalMoney
                        ));

                //serialize best net for future generations
                JsonSerializer serializer = new JsonSerializer();

                if (bestNetwork.Score > 500)
                {
                    serializer.NullValueHandling = NullValueHandling.Include;
                    //serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    serializer.TypeNameAssemblyFormat =
                        FormatterAssemblyStyle.Simple;
                    serializer.TypeNameHandling = TypeNameHandling.Objects;
                    serializer.Formatting = Formatting.Indented;
                    serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

                    using (
                        StreamWriter sw =
                            new StreamWriter(string.Format(@"old_nets/net_{0}.json", population.CurrentGeneration)))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, bestNetwork);
                    }
                }

                if (DoSerialize)
                {
                    DoSerialize = false;

                    File.Delete("SerializedNetworks.json");

                    //population.Networks = population.Networks.OrderByDescending(n => n.Fitness).ToList();

                    //File.WriteAllText(
                    //    "SerializedNetworks.json",
                    //    JsonConvert.SerializeObject(
                    //        population.Networks.First(),
                    //        Formatting.None,
                    //        new JsonSerializerSettings()
                    //        {
                    //            TypeNameHandling = TypeNameHandling.Objects,
                    //            TypeNameAssemblyFormat =
                    //                    System.Runtime.Serialization.Formatters
                    //                    .FormatterAssemblyStyle.Simple,
                    //            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    //        }));
                    serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Include;
                    //serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    serializer.TypeNameAssemblyFormat =
                        System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                    serializer.TypeNameHandling = TypeNameHandling.Objects;
                    serializer.Formatting = Formatting.Indented;
                    serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

                    using (StreamWriter sw = new StreamWriter(@"SerializedNetworks.json"))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, bestNetwork);
                    }
                }

                if (Stop)
                {
                    return;
                }

                population.CalculateNextPopulation();

                foreach (var network in population.Networks)
                {
                    network.Reset();
                }

                if (Stop)
                {
                    return;
                }

                count++;
            }
        }

        private void RunNetworkTrades(
            List<TradeEntry> trades,
            double startEma,
            INetwork network,
            int tradeCount,
            object lockObject,
            ref double bestGenomeFitness,
            ref double bestGenomeMoney,
            ref int bestGenomeBuys,
            ref int bestGenomeSells,
            ref string bestGenomeTradeHistory,
            ref INetwork bestNetwork)
        {
            var usd = 1000.0;
            var bitcoin = 1000.0 / trades.First().MaxPrice;

            TradeEntry previousTrade = trades.First();

            var buyCount = 0;
            var sellCount = 0;

            var buySignalCount = 0;
            var sellSignalCount = 0;

            var profits = 0.0;

            var losses = 0.0;

            var profitCount = 1.0;
            var lossCount = 1.0;

            var netTradeHistory = new List<string>();

            var i = 0;

            TradeEntry currentTrade = trades.First();

            var buys = new Stack<Tuple<double, double>>();

            buys.Push(Tuple.Create(1000.0 / trades.First().MaxPrice, trades.First().MaxPrice));

            var averageMoney = 0.0;

            var currFitness = 0.0;

            var ema = startEma;

            var queue = new Queue<double>();

            for (int j = 0; j < 11; j++)
            {
                queue.Enqueue((trades[j].FirstPrice / ema - 1) * 100);
                ema = ema + 0.08 * (trades[j].FirstPrice - ema);
            }

            foreach (var trade in trades.Skip(10).ToList())
            {
                //if (this.Stop)
                //{
                //    return bestGenomeFitness;
                //}

                if (Math.Abs(trade.MaxPrice / previousTrade.MaxPrice - 1) > 0.5)
                {
                    continue;
                }

                ema = ema + 0.08 * (trade.FirstPrice - ema);

                currentTrade = trade;

                var newEl = (previousTrade.LastPrice / ema - 1) * 100;

                var hist = queue.ToArray();

                network.SetInputs(
                    hist[0],
                    hist[1],
                    hist[2],
                    hist[3],
                    hist[4],
                    hist[5],
                    hist[6],
                    hist[7],
                    hist[8],
                    hist[9],
                    hist[10],
                    newEl,
                    previousTrade.Volume);

                queue.Enqueue(newEl);
                queue.Dequeue();

                var output = network.GetOutputs();

                if (i < 50)
                {
                    previousTrade = trade;
                    i++; //do nothing first 50 rounds so networks can initialize if necessary
                    continue;
                }
                var ignoreSignal = output[0];
                //var ignoreConfidence = output[0];
                var buySignal = output[1];
                //var buyConfidence = output[1];
                var sellSignal = output[2];
                //var sellConfidence = output[2];

                if (buySignal > ignoreSignal && buySignal > sellSignal)
                {
                    buySignalCount++;

                    var amount = buySignal * usd;

                    // buy bitcoin
                    var btcAmount = amount / trade.MaxPrice * 0.998;

                    if (amount <= usd && amount != 0 && btcAmount >= 0.01) // && buyConfidence > 0.2)
                    {
                        netTradeHistory.Add($"B{{{trade.Key:yyyy-MM-dd HH:mm},{amount},{trade.MaxPrice},{usd},{bitcoin}}} ");
                        buyCount++;

                        buys.Push(Tuple.Create(btcAmount, trade.MaxPrice));

                        usd -= amount;

                        bitcoin += btcAmount;
                    }
                }
                else if (sellSignal > ignoreSignal && sellSignal > buySignal)
                {
                    //sell bitcoin
                    sellSignalCount++;

                    var btcAmount = sellSignal * bitcoin;

                    if (btcAmount <= bitcoin && btcAmount != 0 && btcAmount >= 0.01) // && sellConfidence > 0.2)
                    {
                        SellBtcAmount(btcAmount, buys, trade, ref profits, ref losses, ref profitCount, ref lossCount);

                        netTradeHistory.Add(
                            $"S{{{trade.Key:yyyy-MM-dd HH:mm},{btcAmount * trade.MinPrice},{trade.MinPrice},{usd},{bitcoin}}} ");
                        sellCount++;

                        bitcoin -= btcAmount;

                        usd += btcAmount * trade.MinPrice * 0.998;

                        if (!buys.Any())
                        {
                            bitcoin = 0;
                        }
                    }
                }

                //var currAmount = usd + bitcoin * currentTrade.Item4 * 0.998;
                //var holdPrice = 500 + (500.0 / startPrice) * currentTrade.Item4 * 0.998;

                //var difference = currAmount
                //                 - holdPrice;

                //difference *= (1 - i * turnWeightDifference);

                //averageMoney = (averageMoney * (i / (i + 1.0)))
                //               + (difference / (i + 1));

                previousTrade = trade;

                i++;
            }

            var prefProfits = profits;

            SellBtcAmount(bitcoin, buys, currentTrade, ref profits, ref losses, ref profitCount, ref lossCount);

            //profits = (profits - prefProfits) * 0.8 + prefProfits;

            usd += bitcoin * currentTrade.MinPrice * 0.998;

            var totalAmount = usd;

            if (sellCount < 1)
            {
                losses = 1000;
                lossCount = 100000;
            }

            //networks that don't trade at all get no fitness, networks that don't sell ever get a handycap
            //var val = 0.0;//Math.Log(1 + Math.Exp(0.01 * (profits - losses) * startPrice / endPrice));
            //var val = Math.Log(1 + Math.Exp(0.1 * ((10 * (1.0 * buyCount + sellCount) / tradeCount) * Math.Pow(usd / 2000.0, 2))));
            var val = (Math.Pow((1.0 * buyCount + sellCount) / tradeCount, 0.9) * Math.Pow(usd / 2000.0, 3)
                       * Math.Pow(profitCount / lossCount, 1));
            //val += Math.Log(1 + Math.Exp(0.01 * historicalSellSignals));
            //val += Math.Log(1 + Math.Exp(0.01 * historicalIgnoreSignals));
            network.Fitness = val; //Math.Max(0.0, 400 / (1 + Math.Exp(-0.02 * (val - 200))) - 6);

            //if (network.HiddenNodes.Count == 0)
            //{
            //    network.Fitness = Math.Min(network.Fitness/1.5, 4);
            //}

            network.Score = totalAmount;

            lock (lockObject)
            {
                if (network.Fitness > bestGenomeFitness
                    || network.Fitness == bestGenomeFitness && network.Score > bestGenomeMoney)
                {
                    bestGenomeFitness = network.Fitness;
                    bestGenomeBuys = buyCount;
                    bestGenomeSells = sellCount;
                    bestGenomeMoney = network.Score;
                    bestGenomeTradeHistory = string.Join("", netTradeHistory);
                    bestNetwork = network;
                }
            }
        }

        private static void SellBtcAmount(double btcAmount, Stack<Tuple<double, double>> buys, TradeEntry trade, ref double profits, ref double losses, ref double profitCount, ref double lossCount)
        {
            var btcTradeAmount = btcAmount;

            while (btcTradeAmount > 0)
            {
                var currTrade = buys.Pop();

                if (currTrade.Item1 <= btcTradeAmount)
                {
                    var amt = (currTrade.Item1 * trade.MinPrice * 0.998) - (currTrade.Item1 / 0.998 * currTrade.Item2);
                    if (amt > 0)
                    {
                        profits += amt;
                        profitCount += 1;
                    }
                    else
                    {
                        losses += -1 * amt;
                        lossCount += 1;
                    }

                    btcTradeAmount -= currTrade.Item1;

                    if (btcTradeAmount < 0.000001)
                    {
                        btcTradeAmount = 0;
                    }
                }
                else
                {
                    var amt = (btcTradeAmount * trade.MinPrice * 0.998) - (btcTradeAmount / 0.998 * currTrade.Item2);

                    if (amt > 0)
                    {
                        profits += amt;
                        profitCount += 1;
                    }
                    else
                    {
                        losses += -1 * amt;
                        lossCount += 1;
                    }

                    buys.Push(Tuple.Create(currTrade.Item1 - btcTradeAmount, currTrade.Item2));

                    btcTradeAmount = 0;
                }
            }
        }

        public List<TradeEntry> GetTradeEntries(long start, long end, bool ascending)
        {
            long count = 0;

            double startPrice = 0.0;
            double endPrice = 0.0;

            using (StreamReader sr = new StreamReader("btceUSD.csv"))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.

                while ((line = sr.ReadLine()) != null)
                {
                    var sp = line.Split(',');
                    var time = int.Parse(sp[0]);

                    if (time >= start && startPrice == 0.0)
                    {
                        startPrice = double.Parse(sp[1]);
                    }
                    else if (startPrice > 0.0 && time >= end)
                    {
                        endPrice = double.Parse(sp[1]);
                        break;
                    }

                    count++;
                }
            }

            if ((ascending && startPrice >= endPrice) || (!ascending && startPrice < endPrice))
            {
                return null;
            }

            var history = new List<string>();

            using (StreamReader sr = new StreamReader("btceUSD.csv"))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    var time = int.Parse(line.Split(',')[0]);

                    if (time >= start && time < end)
                    {
                        history.Add(line);
                    }
                    else if (time >= end)
                    {
                        break;
                    }

                    count++;
                }
            }

            var unixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            return history.Select(
                l =>
                {
                    var split = l.Split(',');
                    return new Tuple<DateTime, double, double>(
                        unixTime.AddSeconds(int.Parse(split[0])),
                        double.Parse(split[2]),
                        double.Parse(split[1]));
                })
                .OrderBy(t => t.Item1)
                .GroupBy(t => new DateTime(t.Item1.Year, t.Item1.Month, t.Item1.Day, t.Item1.Hour, (t.Item1.Minute / TradePeriod) * TradePeriod, 0))
                .Select(
                    g =>
                    new TradeEntry()
                    {
                        Key = g.Key,
                        FirstPrice = g.First().Item3,
                        LastPrice = g.Last().Item3,
                        MinPrice = g.Min(t => t.Item3),
                        MaxPrice = g.Max(t => t.Item3),
                        Volume = g.Sum(t => t.Item2)
                    })
                .ToList();
        }
    }

    public class Signal
    {
        public int Index;

        public double Price;

        public double Confidence;

        public double Fitness;
    }

    public struct TradeEntry
    {
        public DateTime Key;

        public double FirstPrice;

        public double LastPrice;

        public double MinPrice;

        public double MaxPrice;

        public double Volume;
    }
}