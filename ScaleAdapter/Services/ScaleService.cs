using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ScaleAdapter.Models;

namespace ScaleAdapter.Services
{
    public class ScaleService : IScaleService
    {
        private readonly IConfiguration _config;

        public ScaleService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<ScaleResponse> GetScaleValues(int scaleNo)
        {
            IConfigurationSection myArraySection = _config.GetSection("Scales");
            var itemArray = myArraySection.AsEnumerable().ToList();
            var ip = itemArray.First(x => x.Key == $"Scales:{scaleNo}:Ip").Value;
            var port = itemArray.First(x => x.Key == $"Scales:{scaleNo}:Port").Value;
            var parsePatterns = itemArray.First(x => x.Key == $"Scales:{scaleNo}:ParsePattern").Value;
            var validLength = int.Parse(itemArray.First(x => x.Key == $"Scales:{scaleNo}:ValidLength").Value);
            
            var bytesReceived = new byte[54];
            
            using (var socket = await ConnectSocketAsync(ip, int.Parse(port)))
            {
                if (socket == null)
                {
                    return null;
                }
            
                int bytes;
                var accumulatedString = string.Empty;

                {
                    bytes = socket.Receive(bytesReceived, bytesReceived.Length, 0);
                    accumulatedString += Encoding.ASCII.GetString(bytesReceived, 0, bytes);

                    if (accumulatedString.Length != validLength)
                    {
                       return GenerateErrorResponse($"Invalid scale response lenght (actual: {accumulatedString.Length}, expected {validLength}");
                    }

                    if (!string.IsNullOrEmpty(parsePatterns))
                    {
                        var values = new List<string>();
                        var patterns = parsePatterns.Split(';').Where(x => !string.IsNullOrEmpty(x));
                        foreach (var pattern in patterns)
                        {
                            var skip = int.Parse(pattern.Split(':')[0]);
                            var take = int.Parse(pattern.Split(':')[1]);
                            
                            values.Add(accumulatedString.Substring(skip, take));
                        }
                        
                        return new ScaleResponse
                        {
                            Values = values.ToArray()
                        };
                    }
                    else
                    {
                        return new ScaleResponse
                        {
                            Values = new[]
                            {
                                accumulatedString
                            }
                        };
                    }
                }
            }
        }

        private async Task<Socket> ConnectSocketAsync(string server, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(server), port);
            await socket.ConnectAsync(ipEndPoint);

            return socket;
        }

        private ScaleResponse GenerateErrorResponse(string message)
        {
            return new ScaleResponse
            {
                ErrorMessage = message
            }; 
        }
    }
}