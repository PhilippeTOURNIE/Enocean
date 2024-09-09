using Enocean.Core.Constants;
using System.Globalization;
using Enocean.Core.EEps;

namespace Enocean.Test
{
    [TestClass]
    public class TemperatureSensorTestcs
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var e = new EEP();
        }

        [TestMethod,Timeout(100)]
        public void TestTemperatureRangeMin()
        {
            var eep = new EEP();
            var offset = -40;
            var values = Enumerable.Range(0x01, 11).ToList(); // 0x0B
            for (int i = 0; i < values.Count(); i++)
            {
                var minimum = (int)(i * 10 + offset);
                var maximum = (int)(minimum + 40);
                var profile = eep.FindProfil(new System.Collections.BitArray(0), (RORG)0xA5, 0x02, values[i]);
                Assert.IsTrue(minimum.ToString() == profile?.First()?.Value?.First(x => x.Shortcut! == "TMP").Scale?.Min?.Split('.')[0]);
                Assert.IsTrue(maximum.ToString() == profile.First()?.Value?.First(x => x.Shortcut! == "TMP").Scale?.Max?.Split('.')[0]);
            }
        }
        [TestMethod,Timeout(100)]
        public void TestTemperatureRangeMax()
        {
            var eep = new EEP();
            var offset = -60;
            var values = Enumerable.Range(0x10, 11).ToList(); //0x1B
            for (int i = 0; i < values.Count(); i++)
            {
                var minimum = (int)(i * 10 + offset);
                var maximum = (int)(minimum + 80);
                var profile = eep.FindProfil(new System.Collections.BitArray(0), (RORG)0xA5, 0x02, values[i]);
                Assert.IsTrue(minimum.ToString() == profile?.First().Value?.First(x => x.Shortcut! == "TMP").Scale?.Min?.Split('.')[0]);
                Assert.IsTrue(maximum.ToString() == profile?.First().Value?.First(x => x.Shortcut! == "TMP").Scale?.Max?.Split('.')[0]);
            }
        }

        [TestMethod,Timeout(100)]
        public void Testrest()
        {
            var eep = new EEP();
            var profile = eep.FindProfil(new System.Collections.BitArray(0), (RORG)0xA5, 0x02, 0x20);
            Assert.IsTrue(-10f == float.Parse(profile?.First().Value?.First(x => x.Shortcut! == "TMP").Scale?.Min!, CultureInfo.InvariantCulture));
            Assert.IsTrue(+41.2f == float.Parse(profile?.First().Value?.First(x => x.Shortcut! == "TMP").Scale?.Max!, CultureInfo.InvariantCulture));

            profile = eep.FindProfil(new System.Collections.BitArray(0), (RORG)0xA5, 0x02, 0x30);
            Assert.IsTrue(-40f == float.Parse(profile?.First().Value?.First(x => x.Shortcut! == "TMP").Scale?.Min!, CultureInfo.InvariantCulture));
            Assert.IsTrue(+62.3f == float.Parse(profile?.First().Value?.First(x => x.Shortcut! == "TMP").Scale?.Max!, CultureInfo.InvariantCulture));
        }
    }
}
