using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.HotUpdate;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LZX.MEditor.LZXStatic
{
    public class LZXDllController
    {
        public static void CheckMetaDataDlls(BuildTarget target)
        {
            string dir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            string dirPath = Application.dataPath.Replace("Assets", dir);
            var checker = new MissingMetadataChecker(dirPath, new List<string>());
            switch (target)
            {
                case BuildTarget.Android:
                    CompileDllCommand.CompileDllAndroid();
                    break;
                case BuildTarget.iOS:
                    CompileDllCommand.CompileDllIOS();
                    break;
                case BuildTarget.StandaloneWindows64:
                    CompileDllCommand.CompileDllWin64();
                    break;
            }
            string hotUpdateDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string dllPath = $"{hotUpdateDir}/{dll}";
                bool notAnyMissing = checker.Check(dllPath);
                if(!notAnyMissing)
                {
                    GenerateAll(target);
                    break;
                }
            }
        }
        public static void GenerateAll(BuildTarget target)
        {
            var installer = new HybridCLR.Editor.Installer.InstallerController();
            if (!installer.HasInstalledHybridCLR())
            {
                throw new BuildFailedException($"You have not initialized HybridCLR, please install it via menu 'HybridCLR/Installer'");
            }
            CompileDllCommand.CompileDll(target);
            Il2CppDefGeneratorCommand.GenerateIl2CppDef();
            // 这几个生成依赖HotUpdateDlls
            LinkGeneratorCommand.GenerateLinkXml(target);
            // 生成裁剪后的aot dll
            StripAOTDllCommand.GenerateStripedAOTDlls(target);
            // 桥接函数生成依赖于AOT dll，必须保证已经build过，生成AOT dll
            MethodBridgeGeneratorCommand.GenerateMethodBridgeAndReversePInvokeWrapper(target);
            AOTReferenceGeneratorCommand.GenerateAOTGenericReference(target);
            CopyMetaDataDll2Assets(target);
        }

        private static void CopyMetaDataDll2Assets(BuildTarget target)
        {
            string dir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            string dirPath = Application.dataPath.Replace("Assets", dir);
            string AssetPath = Path.Combine(Application.dataPath, "LZX/AOTDlls");
            if (!Directory.Exists(AssetPath))
                Directory.CreateDirectory(AssetPath);
            string[] files = Directory.GetFiles(dirPath, "*.dll");
            foreach (var dll in files)
            {
                string fileName = Path.GetFileName(dll);
                string destPath = Path.Combine(AssetPath,target.ToString() ,fileName);
                File.Copy(dll, destPath+".bytes", true);
            }
        }
    }
}