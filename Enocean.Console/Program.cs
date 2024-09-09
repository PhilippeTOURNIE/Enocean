using Enocean.Console;
using Enocean.Core;
using Enocean.Core.Constants;
using Enocean.Core.Packets;
using Microsoft.Extensions.Logging;


using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = factory.CreateLogger("Program");
logger.LogInformation("Test Enocean Core");
var MaterialList = new List<Material>() {
    new Material {Description="Contact", Rorg =RORG.BS1, RorgFunc =0x00 ,RorgType = 0x01 , Sender = [0x05,0x95,0xFE,0x58]  },
    new Material {Description="Push Button", Rorg =RORG.RPS, RorgFunc =0x01 ,RorgType = 0x01 , Sender = [0x00,0x32,0xCE,0xA5]  },
    new Material {Description="Occupancy", Rorg =RORG.BS4, RorgFunc =0x07 ,RorgType = 0x01 , Sender = [0x05,0x17,0xD2,0x54]  }};

Action<Packet?> display = (p) =>
{
    if (p.PacketType == PACKET.RESPONSE)
    {
        var responseBytes = ((ResponsePacket)p).ResponseData;
        Console.WriteLine($"responseBytes : {Utils.ToHexString(responseBytes)}");
    }
    if (p is UTETeachInPacket utep)
    {
        if ( !MaterialList.Any(m => m.Sender.SequenceEqual(utep.Sender)))
        {
            var description = utep.EEPObject.DiscoverFromRorg(utep.RorgOfEep, utep.RorgFunc, utep.RorgType);
            MaterialList.Add(new Material { Description = description??"", Rorg = utep.RorgOfEep, RorgFunc = utep.RorgFunc, RorgType = utep.RorgType, Sender = utep.Sender });
        } 
    }
    if (p is RadioPacket rp)
    {
        // search in identifier material
        var material = MaterialList.FirstOrDefault(m => Utils.ToHexString(m.Sender) == Utils.ToHexString(rp.Sender));
        if (material != null)
        {
            // material 
            material.State = rp.ParseEEP(material.RorgFunc, material.RorgType);
            Console.WriteLine("*************************************************************");
            Console.Write(material.ToString());
            Console.WriteLine("*************************************************************");

        }
        else
        {
            //unknow material
            var materialsPossible = p.EEPObject.DiscoverFromRorg(p.Rorg);
            if (materialsPossible?.Any() == true)
            {
                Console.WriteLine($"Sender address : {rp.SenderToHex}");
                Console.WriteLine($"Rorg : {p.Rorg}");
                foreach (var rFunc in materialsPossible)
                {
                    foreach (var rType in rFunc.Value)
                    {
                        Console.WriteLine($" Func : {rFunc.Key} Type : {rType.Key} Description : {rType.Value}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Unknow Material: {p.ToString()}");
            }
            Console.WriteLine("*************************************************************");
        }
    }
};



var ser = new Enocean.Core.Communicators.SerialCommunicator("COM7", display, logger);
ser.OpenConnection();
Console.WriteLine("*************************************************************");
ser.BaseId = [0x05, 0xA1, 0xDB, 0xFC];
ser.TeachIn = true;

var ct = new CancellationToken();
var taskEnocean = Task.Run(() =>
{
    ser.Run();
}, ct);

await Task.Delay(500);

Console.WriteLine("Wait sending push button");
Console.ReadKey();

var sender = new List<byte>() { 0x05, 0xA1, 0xFD, 0xFC };
var dest = new List<byte>() { 0x05, 0x91, 0xDF, 0xC6 };

var prop = new Dictionary<string, object>();
prop["POS"] = 100;
prop["ANG"] = 127;
prop["REPO"] = 0;
prop["LOCK"] = 0;
var packetSendCmd = RadioPacket.CreateRadio(RORG.VLD, 0x05, 0x00, null, 1, dest, sender, false, prop);
Console.WriteLine(" sending ");
ser.Send(packetSendCmd);

Console.WriteLine("Wait for disable button sending");
Console.ReadKey();
prop["POS"] = 0;
prop["ANG"] = 127;
prop["REPO"] = 0;
prop["LOCK"] = 0;
 packetSendCmd = RadioPacket.CreateRadio(RORG.VLD, 0x05, 0x00, null, 1, dest, sender, false, prop);
ser.Send(packetSendCmd);
Console.WriteLine("please key for ended");
Console.ReadKey();



