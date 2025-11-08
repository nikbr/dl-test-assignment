

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Q1
{
    public class IPRangeDictionary : SortedDictionary<IPAddress, IPRange> // словарь сохраняет добавленый IP и диапазон которому принадлежит 
    {
        SortedSet<IPRange> ranges = new SortedSet<IPRange>();   //структура для принта
        public void Push(IPAddress address)
        {
            if (!this.ContainsKey(address))
            {
                IPAddress left;
                IPAddress right;
                if (address.EndsWithOne())
                {
                    left = new IPAddress(address.Value - 2); //пропускаем 0
                }
                else
                {
                    left = new IPAddress(address.Value - 1);
                }
                if (address.EndsWith255())
                {
                    right = new IPAddress(address.Value + 2); //пропускаем 0
                }
                else
                {
                    right = new IPAddress(address.Value + 1);
                }

                    
                
                if (address.Value > 0 && this.ContainsKey(left) && address.Value < uint.MaxValue && this.ContainsKey(right)) //соединяет два диапазона
                {

                    var leftRange = this[left];
                    var rightRange = this[right];

                    if (leftRange.Length() > rightRange.Length())
                    {
                        var newUpper = rightRange.Upper;
                        leftRange.Add(newUpper);
                        ranges.Remove(rightRange);
                        this.Add(address, leftRange);

                        for (uint i = address.Value+1; i <= newUpper.Value; i++)
                        {
                            var addressInRange = new IPAddress(i);
                            this[addressInRange] = leftRange;
                        }
                    }
                    else
                    {
                        var newLower = leftRange.Lower;
                        rightRange.Add(newLower);
                        ranges.Remove(leftRange);
                        this.Add(address, rightRange);

                        for (uint i = newLower.Value; i < address.Value; i++)
                        {
                            var addressInRange = new IPAddress(i);
                            this[addressInRange] = rightRange;
                        }
                    }
                }else if(address.Value > 0 && this.ContainsKey(left)) //присоединяет к диапазону с лева от адреса 
                {
                    var leftRange = this[left];
                    var newLower = leftRange.Lower;
                    leftRange.Add(address);
                    this.Add(address, leftRange);
                }
                else if(address.Value < uint.MaxValue && this.ContainsKey(right)) //присоединяет к диапазону с права от адреса
                {
                    var rightRange = this[right];
                    var newUpper = rightRange.Upper;
                    rightRange.Add(address);
                    this.Add(address, rightRange);
                }
                else
                {
                    var newRange = new IPRange(address);
                    ranges.Add(newRange);
                    this.Add(address, newRange);
                }

            }
        }

        public void Print()
        {
            foreach (var range in ranges)
            {
                range.Print();
            }

            
        }

    }

    // Диапазоны, на пример: 1.1.1.1-1.1.1.255
    public class IPRange : IComparable<IPRange>
    {
        public IPAddress Lower;
        public IPAddress Upper;

        public IPRange(IPAddress address)
        {
            Lower = address;
            Upper = address;
        }
        public IPRange(IPAddress lower, IPAddress upper)
        {
            if (upper<lower)
                    throw new Exception("Invalid IP Range");
            Lower = lower;
            Upper = upper;
        }

        public void Add(IPAddress newAddress){
                if (newAddress > Upper)
            {
                Upper = newAddress;
            }
            else if(newAddress < Lower){
                Lower = newAddress;
            }
        }


        public int CompareTo(IPRange other)
        {
            if (Lower.Value == other.Lower.Value)
            {
                return Upper.Value.CompareTo(other.Upper.Value);
            }
            else
            {
                return Lower.Value.CompareTo(other.Lower.Value);
            }
        }
        public void Print()
        {
            if (Lower != Upper)
            {
                Console.WriteLine(Lower.ToString() + " - " + Upper.ToString());
            }
            else
            {
                Console.WriteLine(Lower.ToString());
            }
               
        }
        public uint Length()
        {
            return Upper.Value - Lower.Value + 1;
        }
    }
    public readonly struct IPAddress : IComparable<IPAddress>
        {
        public uint Value { get; }
        public IPAddress(IPAddress? other)
        {
            Value = other?.Value ?? 0u;

        }
        
        public IPAddress(uint value)
        {
            Value = value;
        }
        public IPAddress(string ip)
        {

            try
            {
                var parts = ip.Split('.');
                if (parts.Length != 4)
                    throw new FormatException();
                byte p1 = byte.Parse(parts[0]);
                byte p2 = byte.Parse(parts[1]);
                byte p3 = byte.Parse(parts[2]);
                byte p4 = byte.Parse(parts[3]);

                Value = (uint)((p1 << 24) | (p2 << 16) | (p3 << 8) | p4);
            }
            catch (Exception)
            {
                Console.WriteLine("Неправильный IPv4 формат.");
                throw new FormatException("Неправильный IPv4 формат.");
            }

        }

        public override string ToString() => $"{(Value >> 24) & 0xFF}.{(Value >> 16) & 0xFF}.{(Value >> 8) & 0xFF}.{Value & 0xFF}";
        


        public int CompareTo(IPAddress other) => Value.CompareTo(other.Value);

        public bool EndsWithZero()
        {
            uint lastPart = Value & 0xFF;
            return lastPart == 0;
        }

        public bool EndsWithOne()
        {
            uint lastPart = Value & 0xFF;
            return lastPart == 1;
        }

        public bool EndsWith255()
        {
            uint lastPart = Value & 0xFF;
            return lastPart == 255;
        }

        public static bool operator >(IPAddress left, IPAddress right) => left.Value > right.Value;
        public static bool operator <(IPAddress left, IPAddress right) => left.Value < right.Value;
        public static bool operator >=(IPAddress left, IPAddress right) => left.Value >= right.Value;
        public static bool operator <=(IPAddress left, IPAddress right) => left.Value <= right.Value;
        public static bool operator ==(IPAddress left, IPAddress right) => left.Value==right.Value;
        public static bool operator !=(IPAddress left, IPAddress right) => left.Value!=right.Value;

    }
}
