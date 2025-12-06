using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace SchemeTool
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load database
            using (Stream stream = File.OpenRead(".\\GameData\\Formats.dat"))
            {
                GameRes.FormatCatalog.Instance.DeserializeScheme(stream);
            }
#if false
            using (Stream stream = File.Create(".\\GameData\\Formats.json"))
            {
                GameRes.FormatCatalog.Instance.SerializeSchemeJson(stream);
                return;
            }
#endif
            GameRes.Formats.KiriKiri.Xp3Opener format = GameRes.FormatCatalog.Instance.ArcFormats
                .FirstOrDefault(a => a is GameRes.Formats.KiriKiri.Xp3Opener) as GameRes.Formats.KiriKiri.Xp3Opener;

            if (format != null)
            {
                GameRes.Formats.KiriKiri.Xp3Scheme scheme = format.Scheme as GameRes.Formats.KiriKiri.Xp3Scheme;

                // Add scheme information here

#if true
                // 读取控制块文件
                byte[] cb = File.ReadAllBytes(@"SICKLYDAYS.bin");  // 改为SICKLYDAYS.bin
                var cb2 = MemoryMarshal.Cast<byte, uint>(cb);
                for (int i = 0; i < cb2.Length; i++)
                    cb2[i] = ~cb2[i];
                var cs = new GameRes.Formats.KiriKiri.CxScheme
                {
                    Mask = 0x2f2,  // 更新mask
                    Offset = 0x2c5,  // 更新offset
                    PrologOrder = new byte[] { 0, 2, 1 },  // 更新PrologOrder
                    OddBranchOrder = new byte[] { 2, 5, 1, 0, 4, 3 },  // 更新OddBranchOrder
                    EvenBranchOrder = new byte[] { 7, 6, 1, 4, 0, 5, 2, 3 },  // 更新EvenBranchOrder
                    ControlBlock = cb2.ToArray()
                };
                var crypt = new GameRes.Formats.KiriKiri.HxCrypt(cs);
                crypt.RandomType = 0;  // 更新randtype
                crypt.FilterKey = 0x975d6564faba3967;  // 更新filterkey
                crypt.NamesFile = "SICKLYDAYS.lst";  // 改为SICKLYDAYS.lst
                var keyA1 = SoapHexBinary.Parse("ea9a25a05369eef3dd9d7b24899145528804e5113d60446024d93a3400ae5307").Value;  // 更新key
                var keyA2 = SoapHexBinary.Parse("d22226c626446fc2a4fc7075ba4822ba").Value;  // 更新nonce
                var keyB1 = SoapHexBinary.Parse("ea9a25a05369eef3dd9d7b24899145528804e5113d60446024d93a3400ae5307").Value;  // 使用相同的key
                var keyB2 = SoapHexBinary.Parse("d22226c626446fc2a4fc7075ba4822ba").Value;  // 使用相同的nonce
                crypt.IndexKeyDict = new Dictionary<string, GameRes.Formats.KiriKiri.HxIndexKey>()
                {
                    { "SICKLYDAYS.bin", new GameRes.Formats.KiriKiri.HxIndexKey { Key1 = keyA1, Key2 = keyA2 } },  // 改为SICKLYDAYS.bin
                    // 如果游戏有其他加密文件，可以添加到这里
                    // { "update.xp3", new GameRes.Formats.KiriKiri.HxIndexKey { Key1 = keyB1, Key2 = keyB2 } },
                };
                
                // 添加方案到已知方案列表，使用游戏名"SICKLYDAYS"作为标识符
                scheme.KnownSchemes.Add("SICKLYDAYS", crypt);
#else
                GameRes.Formats.KiriKiri.ICrypt crypt = new GameRes.Formats.KiriKiri.XorCrypt(0x00);
#endif

            }

            var gameMap = typeof(GameRes.FormatCatalog).GetField("m_game_map", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(GameRes.FormatCatalog.Instance) as Dictionary<string, string>;

            if (gameMap != null)
            {
                // 添加游戏执行文件映射到方案名称
                gameMap.Add("sickly_days.exe", "SICKLYDAYS");
            }

            // Save database
            using (Stream stream = File.Create(".\\GameData\\Formats.dat"))
            {
                GameRes.FormatCatalog.Instance.SerializeScheme(stream);
            }
        }
    }
}
