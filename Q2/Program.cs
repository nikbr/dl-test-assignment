using Q2;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;


var trie4 = new CIDRTrie();
var trie6 = new CIDRTrie(); //Два дерева для быстрого поиска.

/* 1. Uncomment this for producer-consumer pattern
var channel = Channel.CreateBounded<string>(
        new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait
        }
    );




var producer = Task.Run(() => Produce(channel.Writer));
var consumer = Task.Run(() => Consume(channel.Reader, trie4, trie6));

await Task.WhenAll(producer, consumer);*/


Populate(trie4, trie6); //2. Comment this for producer-consumer pattern


Console.WriteLine("Все адреса загружены! Наберите адрес, чтобы узнать страну и штат!\n" +
    "Напишите quit, чтобы выйти!");
while (true)
{

    string? input = Console.ReadLine();
    if (input.Trim().ToLower() == "quit")
    {
        break;
    }
    else
    {
        try
        {
            var ip = IPAddress.Parse(input.Trim());
            CIDRMetadata? result;
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                result = trie4.Search(ip);
            }
            else
            {
                result = trie6.Search(ip);
            }

            if (result == null)
            {
                Console.WriteLine("Адрес не найден.");
            }
            else
                result?.Print();

        }
        catch (Exception)
        {
            Console.WriteLine("Не валидный IP.");
        }
    }
}
static (IPAddress, int) ParseCidrRange(string cidrRange)
{
    var parts = cidrRange.Split("/");
    var ip = IPAddress.Parse(parts[0]);
    var prefixLength = int.Parse(parts[1]);
    return (ip, prefixLength);
}
static void Populate(CIDRTrie trie4, CIDRTrie trie6)
{
    using (var sr = new StreamReader("geo-US.csv"))
    {
        int counter = 0;
        //var sw = System.Diagnostics.Stopwatch.StartNew();
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            var parts = line.Split(',');


            var cidrRange = parts[0].Trim();
            var CountryCode = parts[3].Trim();
            var CountryName = parts[4].Trim();
            var StateCode = parts[5].Trim();
            var StateName = parts[6].Trim();
            var metadata = new CIDRMetadata(cidrRange, CountryCode, CountryName, StateCode, StateName);

            try
            {
                var (ip, prefixLength) = ParseCidrRange(cidrRange);
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    trie4.Insert(ip, prefixLength, metadata);
                }
                else
                {
                    trie6.Insert(ip, prefixLength, metadata);
                }
                if (counter % 100000 == 0)
                {
                    Console.WriteLine("Processed " + counter);
                }
                counter++;
            }
            catch { }
        }
       // sw.Stop();
       // Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds} ms");
    }
}
static async Task Produce(ChannelWriter<string> writer) //Проводил эксперимент что будет быстрее, просто StreamReader или Produce-Consume
{

    using (var sr = new StreamReader("geo-US.csv"))
    {
        string? line;
        do
        {
            line = await sr.ReadLineAsync();
            if (line == null) break;
            if (line == "") continue;
            await writer.WriteAsync(line);

        } while(true);

        writer.Complete();
    }
}

static async Task Consume(ChannelReader<string> reader, CIDRTrie trie4, CIDRTrie trie6)
{
    int counter = 0;
   // var sw = System.Diagnostics.Stopwatch.StartNew();

    while (await reader.WaitToReadAsync())
    {
        var line = await reader.ReadAsync();
        var parts = line.Split(',');


        var cidrRange = parts[0].Trim();
        var CountryCode = parts[3].Trim();
        var CountryName = parts[4].Trim();
        var StateCode = parts[5].Trim();
        var StateName = parts[6].Trim();
        var metadata = new CIDRMetadata(cidrRange, CountryCode,CountryName,StateCode,StateName);

        try
        {
            var (ip, prefixLength) = ParseCidrRange(cidrRange);
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                trie4.Insert(ip, prefixLength, metadata);
            }
            else
            {
                trie6.Insert(ip, prefixLength, metadata);
            }
            if (counter % 100000 == 0)
            {
                Console.WriteLine("Processed " + counter);
            }
            counter++;
        }
        catch { }

    }
  //  sw.Stop();
   // Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds} ms");
}



