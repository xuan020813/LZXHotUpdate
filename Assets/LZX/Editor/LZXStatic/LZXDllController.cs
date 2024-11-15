using System;
using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.HotUpdate;
using LZX.MEditor.MScriptableObject;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LZX.MEditor.LZXStatic
{
    public class LZXDllController
    {
        /// <summary>
        /// 需要重新生成时返回true，否则返回false
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CheckMetaDataDlls(BuildTarget target)
        {
            string dir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            string dirPath = Application.dataPath.Replace("Assets", dir);
            var checker = new MissingMetadataChecker(dirPath, new List<string>());
            string hotUpdateDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string dllPath = $"{hotUpdateDir}/{dll}";
                bool notAnyMissing = checker.Check(dllPath);
                if(!notAnyMissing)
                {
                    GenerateAll(target);
                    return true;
                }
            }
            return false;
        }
        public static void GenerateAll(BuildTarget target)
        {
            var installer = new HybridCLR.Editor.Installer.InstallerController();
            if (!installer.HasInstalledHybridCLR())
            {
                throw new BuildFailedException($"You have not initialized HybridCLR, please install it via menu 'HybridCLR/Installer'");
            }
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
            string AssetPath = Path.Combine(Application.dataPath, "LZX/AOTDlls",target.ToString());
            if (!Directory.Exists(AssetPath))
                Directory.CreateDirectory(AssetPath);
            string[] files = Directory.GetFiles(dirPath, "*.dll");
            foreach (var dll in files)
            {
                string fileName = Path.GetFileName(dll);
                string destPath = Path.Combine(AssetPath,fileName);
                File.Copy(dll, destPath+".bytes", true);
            }
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// 需要重新生成时返回true，否则返回false
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CheckGenerate(BuildTarget target)
        {
            CompileDllCommand.CompileDll(target);
            string dir = Application.dataPath.Replace("Assets", SettingsUtil.GetAssembliesPostIl2CppStripDir(target));
            if (Directory.Exists(dir) && Directory.GetFiles(dir).Length > 0)
            {
                return CheckMetaDataDlls(target);
            }
            GenerateAll(target);
            return true;
        }
        public static Bundle GetMetaDataBundle(BuildTarget target)
        {
            string dir = Path.Combine(Application.dataPath, "LZX/AOTDlls", target.ToString());
            if (!Directory.Exists(dir))
                throw new Exception("Assets/LZX/AOTDlls not exists");
            var bundle = ScriptableObject.CreateInstance<Bundle>();
            bundle.Name = "MetaDataDlls";
            List<string> guid = new List<string>();
            foreach (var dll in Directory.GetFiles(dir))
            {
                if(!dll.EndsWith(".bytes"))
                    continue;
                string assetPath = dll.Replace(Application.dataPath, "Assets");
                guid.Add(AssetDatabase.AssetPathToGUID(assetPath));
            }
            bundle.AssetGUIDs = guid;
            return bundle;
        }
        public static void CopyDll2OutPut(LZXBundleBuild.LZXBundleBuildParameter parameter)
        {
            string HyDllPath = Application.dataPath.Replace("Assets",SettingsUtil.GetHotUpdateDllsOutputDirByTarget(parameter.Target));
            foreach (var asmdef in LZXEditorResources.GetSetting().asmdefs)
            {
                string hydllpath = Path.Combine(HyDllPath, asmdef.name + ".dll");
                if (!File.Exists(hydllpath))
                    Debug.LogError($"HotUpdateDll:{hydllpath}不存在");
                File.Copy(hydllpath, Path.Combine(parameter.OutputPath, asmdef.name + ".dll.bytes"), true);
            }
        }

        public static Bundle[] GetHotUpdateButNonHotUpdateLogicDlls(BuildTarget target)
        {
            string distPath = Path.Combine(Application.dataPath, "LZX/HotUpdateDlls", target.ToString());
            if (Directory.Exists(distPath))
                Directory.Delete(distPath, true);
            Directory.CreateDirectory(distPath);
            string HyDllPath = Application.dataPath.Replace("Assets",SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target));
            foreach (var asmdef in LZXEditorResources.GetSetting().asmdefs)
            {
                string hydllpath = Path.Combine(HyDllPath, asmdef.name + ".dll");
                if (!File.Exists(hydllpath))
                    Debug.LogError($"HotUpdateDll:{hydllpath}不存在");
                File.Copy(hydllpath, Path.Combine(distPath, asmdef.name + ".dll.bytes"), true);
            }
            AssetDatabase.Refresh();
            List<Bundle> bundles = new List<Bundle>();
            foreach (var dll in Directory.GetFiles(distPath))
            {
                if(!dll.EndsWith(".bytes") || dll.Contains("HotUpdate"))
                    continue;
                Bundle bundle = ScriptableObject.CreateInstance<Bundle>();
                bundle.Name = Path.GetFileName(dll);
                string assetPath = dll.Replace(Application.dataPath, "Assets");
                bundle.GUID = AssetDatabase.AssetPathToGUID(assetPath);
                bundles.Add(bundle);
            }
            return bundles.ToArray();
        }
    }
}