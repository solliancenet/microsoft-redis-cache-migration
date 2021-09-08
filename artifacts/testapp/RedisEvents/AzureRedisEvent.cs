using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace RedisEvents
{
    public class AzureRedisEvent
    {
        public readonly string NotificationType;
        public readonly DateTime StartTimeInUTC;
        public readonly DateTimeOffset StartTimeOffset;
        public readonly bool IsReplica;
        public readonly IPAddress IpAddress;
        public readonly int SSLPort;
        public readonly int NonSSLPort;

        public AzureRedisEvent(string message)
        {
            try
            {
                var info = message?.Split('|');

                for (int i = 0; i < info?.Length / 2; i++)
                {
                    string key = null, value = null;
                    if (2 * i < info.Length) { key = info[2 * i].Trim(); }
                    if (2 * i + 1 < info.Length) { value = info[2 * i + 1].Trim(); }
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {
                        switch (key.ToLowerInvariant())
                        {
                            case "notificationtype":
                                NotificationType = value;
                                break;
                            case "starttimeinutc":
                                DateTimeOffset.TryParse(value, out StartTimeOffset);
                                break;
                            case "isreplica":
                                bool.TryParse(value, out IsReplica);
                                break;
                            case "ipaddress":
                                System.Net.IPAddress.TryParse(value, out IpAddress);
                                break;
                            case "sslport":
                                Int32.TryParse(value, out var port);
                                break;
                            case "nonsslport":
                                Int32.TryParse(value, out var nonsslport);
                                break;
                            default:
                                Console.WriteLine($"Unexpected i={i}, case {key}");
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }
        
    }
}
