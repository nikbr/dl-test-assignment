using Q1;
Console.WriteLine("Привет! Это первое задание из тестового. Можете добавить IP в input.txt или добавляйте по stdin!" +
    "\nПишите quit, чтобы выйти!" +
    "\nПишите print, чтобы перечислить все IP адресса.");


IPAddress? SmallestIP = null;
IPAddress? SmallestFreeIP = null;
// словарь добавленых IP
var AllIPs = new IPRangeDictionary();

if (File.Exists("input.txt"))
{
    string[] lines = File.ReadAllLines("input.txt");
    foreach (var line in lines)
    {
        var CurIP = new IPAddress(line.Trim());
        if (CurIP.EndsWithZero())
        {
            Console.WriteLine("IP заканчивается на 0 и не добавлен.");
        }
        else
        {
            AllIPs.Push(CurIP);
            if (SmallestIP == null)
            {
                SmallestIP = CurIP;
            }
            else
            {
                if (CurIP < SmallestIP)
                {
                    SmallestIP = CurIP;
                }
            }

            UpdateSmallestFreeIP();
        }
    }


    if(SmallestFreeIP == null && lines.Length>0)
    {
        Console.WriteLine("Нет свободных IP.");
    }
    else if (SmallestFreeIP != null) 
    {
        Console.WriteLine("Первый свободный IP: " + SmallestFreeIP.ToString());
    }

}
while (true)
{

    string? input = Console.ReadLine();
    // quit command
    if (input.Trim().ToLower() == "quit")
    {
        break;
    }
    //print command
    else if (input.Trim().ToLower() == "print")
    {
        AllIPs.Print();
    }
    else
    {
        try
        {
            var newAddress = new IPAddress(input.Trim());
            if (newAddress.EndsWithZero())
            {
                Console.WriteLine("IP заканчивается на 0 и не добавлен.");
            }
            else { 
                AllIPs.Push(newAddress);
                if (SmallestIP == null || newAddress < SmallestIP)
                {
                    SmallestIP = newAddress;
                }
                UpdateSmallestFreeIP();
                if (SmallestFreeIP == null)
                {
                    Console.WriteLine("Нет свободных IP.");
                }
                else
                {
                    Console.WriteLine("Первый свободный IP: " + SmallestFreeIP.ToString());
                }
                
            }
                
           
        }catch(Exception)
        {
            Console.WriteLine("Не валидный IP.");
        }
    }
}


void UpdateSmallestFreeIP()
{
    IPAddress smallestIPAlias = new IPAddress(SmallestIP);
    var Range = AllIPs[smallestIPAlias];
    var Upper = Range.Upper;
    while (Upper.Value < uint.MaxValue)
    {
        var smallestFreeIPAlias = new IPAddress(Upper.Value + 1);
        
        if (smallestFreeIPAlias.EndsWithZero())
        {
            Upper = new IPAddress(Upper.Value + 2);
            if (!AllIPs.ContainsKey(Upper))
            {
                SmallestFreeIP = new IPAddress(Upper.Value + 2);
                break;
            }
            else
            {
                Range = AllIPs[Upper];
                Upper = Range.Upper;
            }
            
        }
        else
        {
            SmallestFreeIP = smallestFreeIPAlias;
            break;
        }
    }
}


    



