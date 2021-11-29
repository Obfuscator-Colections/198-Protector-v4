﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Confuser.Runtime
{
	// Token: 0x02000010 RID: 16
	internal static class Compressor
	{
		// Token: 0x0600005D RID: 93 RVA: 0x000048F8 File Offset: 0x00002AF8
		private static GCHandle Decrypt(uint[] data, uint seed)
		{
			uint[] array = new uint[16];
			uint[] array2 = new uint[16];
			ulong num = (ulong)seed;
			for (int i = 0; i < 16; i++)
			{
				num = num * num % 339722377UL;
				array2[i] = (uint)num;
				array[i] = (uint)(num * num % 1145919227UL);
			}
			Mutation.Crypt(array, array2);
			Array.Clear(array2, 0, 16);
			byte[] array3 = new byte[data.Length << 2];
			uint num2 = 0U;
			for (int j = 0; j < data.Length; j++)
			{
				uint num3 = data[j] ^ array[j & 15];
				array[j & 15] = (array[j & 15] ^ num3) + 1037772825U;
				array3[(int)num2] = (byte)num3;
				array3[(int)(num2 + 1U)] = (byte)(num3 >> 8);
				array3[(int)(num2 + 2U)] = (byte)(num3 >> 16);
				array3[(int)(num2 + 3U)] = (byte)(num3 >> 24);
				num2 += 4U;
			}
			Array.Clear(array, 0, 16);
			byte[] array4 = Lzma.Decompress(array3);
			Array.Clear(array3, 0, array3.Length);
			GCHandle result = GCHandle.Alloc(array4, GCHandleType.Pinned);
			uint num4 = (uint)(num % 9067703UL);
			for (int k = 0; k < array4.Length; k++)
			{
				byte[] array5 = array4;
				int num5 = k;
				array5[num5] ^= (byte)num;
				if ((k & 255) == 0)
				{
					num = num * num % 9067703UL;
				}
			}
			return result;
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00004A38 File Offset: 0x00002C38
		[STAThread]
		private static int Main(string[] args)
		{
			uint keyI = (uint)Mutation.KeyI0;
			uint[] array = Mutation.Placeholder<uint[]>(new uint[Mutation.KeyI0]);
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			Module manifestModule = executingAssembly.ManifestModule;
			GCHandle gchandle = Compressor.Decrypt(array, (uint)Mutation.KeyI1);
			byte[] array2 = (byte[])gchandle.Target;
			Module module = executingAssembly.LoadModule("What_A_Fucking_Joke_Am_I_Right", array2);
			Array.Clear(array2, 0, array2.Length);
			gchandle.Free();
			Array.Clear(array, 0, array.Length);
			Compressor.key = manifestModule.ResolveSignature(Mutation.KeyI2);
			AppDomain.CurrentDomain.AssemblyResolve += Compressor.Resolve;
			module.GetTypes();
			MethodBase methodBase = module.ResolveMethod((int)Compressor.key[0] | (int)Compressor.key[1] << 8 | (int)Compressor.key[2] << 16 | (int)Compressor.key[3] << 24);
			object[] array3 = new object[methodBase.GetParameters().Length];
			if (array3.Length != 0)
			{
				array3[0] = args;
			}
			object obj = methodBase.Invoke(null, array3);
			if (obj is int)
			{
				return (int)obj;
			}
			return 0;
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00004B48 File Offset: 0x00002D48
		private static Assembly Resolve(object sender, ResolveEventArgs e)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(new AssemblyName(e.Name).FullName.ToUpperInvariant());
			Stream stream = null;
			if (bytes.Length + 4 <= Compressor.key.Length)
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					byte[] array = bytes;
					int num = i;
					array[num] *= Compressor.key[i + 4];
				}
				string name = Convert.ToBase64String(bytes);
				stream = Assembly.GetEntryAssembly().GetManifestResourceStream(name);
			}
			if (stream != null)
			{
				uint[] array2 = new uint[stream.Length >> 2];
				byte[] array3 = new byte[256];
				int num2 = 0;
				int num3;
				while ((num3 = stream.Read(array3, 0, 256)) > 0)
				{
					Buffer.BlockCopy(array3, 0, array2, num2, num3);
					num2 += num3;
				}
				uint num4 = 7339873U;
				foreach (byte b in bytes)
				{
					num4 = num4 * 6176543U + (uint)b;
				}
				GCHandle gchandle = Compressor.Decrypt(array2, num4);
				byte[] array5 = (byte[])gchandle.Target;
				Assembly result = Assembly.Load(array5);
				Array.Clear(array5, 0, array5.Length);
				gchandle.Free();
				Array.Clear(array2, 0, array2.Length);
				return result;
			}
			return null;
		}

		// Token: 0x04000031 RID: 49
		private static byte[] key;
	}
}
