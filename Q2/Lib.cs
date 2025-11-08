
using System.Net;

namespace Q2
{
    struct CIDRMetadata
    {
        public string Address;
        public string CountryCode;
        public string CountryName;
        public string StateCode;
        public string StateName;
        public CIDRMetadata(string a, string cc, string cn, string sc, string sn)
        {
            Address = a;
            CountryCode = cc; 
            CountryName = cn;
            StateCode = sc;
            StateName = sn;
        }

        public void Print()
        {
            Console.WriteLine($"Адрес находится в: \n" +
            $"{CountryCode},{CountryName},{StateCode},{StateName}" +
            $"в диапазоне {Address}");
        }
    } 

    class CIDRTrie
    {
        private readonly CIDRTrieNode root = new();
        public void Insert(IPAddress ip, int prefixLength, CIDRMetadata metadata)
        {
            var bits = IPToBits(ip);
            var node = root;
            for (int i = 0; i < prefixLength; i++){
                bool bit = bits[i];
                if (bit)
                {
                    if (node.TChild==null) node.TChild = new CIDRTrieNode();
                    node = node.TChild;
                }
                else
                {
                    if (node.FChild == null) node.FChild = new CIDRTrieNode();
                    node = node.FChild;
                }
            }
            node.Metadata = metadata;

        }

        public CIDRMetadata? Search(IPAddress ip)
        {
            var bits = IPToBits(ip);
            var node = root;
            CIDRMetadata? match = null;
            foreach(bool bit in bits)
            {
                if (node.Metadata != null)
                {
                    match = node.Metadata;
                }

                if (bit)
                {
                    if (node.TChild == null) break;
                    node = node.TChild;
                }
                else
                {
                    if (node.FChild == null) break;
                    node = node.FChild;
                }
            }


            return match;

        }

        private static bool[] IPToBits(IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();

            bool[] bits = new bool[bytes.Length*8];

            for (int i = 0; i < bytes.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bool bit = (bytes[i] & (1 << (7 - j))) != 0;
                    bits[i * 8 + j] = bit;
                }

            }

            return bits;
        }

    }

    class CIDRTrieNode
    {
        public CIDRMetadata? Metadata { get; set; }
        public CIDRTrieNode? FChild { get; set; } //0
        public CIDRTrieNode? TChild { get; set; } //1
    }
}
