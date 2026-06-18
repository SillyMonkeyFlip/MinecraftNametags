using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MinecraftNametags;
using GorillaLibrary.Utilities;
using GorillaLibrary.Models;
using MinecraftNametags.Behaviours;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;

namespace MinecraftNametags;

[BepInPlugin("com.skye.minecraftnametags", "Minecraft Nametags", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public const string SIGNIFICANCE_URL =
        "https://raw.githubusercontent.com/skyedotnet/MinecraftNametags/refs/heads/main/significance.txt";

    public static Plugin Instance;
    public AssetBundle Bundle;

    public Dictionary<string, Significance> SignificanceMapping;

    private void Awake()
    {
        SignificanceMapping = new Dictionary<string, Significance>();
        SetupSignificance();

        Instance = this;

        Bundle = AssetBundleUtility.LoadBundle(System.Reflection.Assembly.GetExecutingAssembly(), "MinecraftNametags.Resources.minecraftnametags");

        GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);
    }

    public void SetupSignificance()
    {
        GorillaLibrary.Utilities.WebRequestUtility.SendRequest(SIGNIFICANCE_URL, new WebRequest()
        {
            Method = RequestMethod.Get,
        }, (string result) =>
        {
            string[] lines = result.Split("\n");
            foreach (string line in lines)
            {
                try
                {
                    string UID = line.Split(':')[0];
                    string SIGNIFICANCE = line.Split(':')[1];

                    switch (SIGNIFICANCE)
                    {
                        case "DYE":
                            SignificanceMapping[UID] = Significance.Boyfriend;
                            break;
                        case "GOLD":
                            SignificanceMapping[UID] = Significance.Golden;
                            break;
                        case "CMD":
                            SignificanceMapping[UID] = Significance.Developer;
                            break;
                        case "DIAMOND":
                            SignificanceMapping[UID] = Significance.Known;
                            break;
                    }
                }
                catch { } //I DON'T KNOW WHAT IT IS, AND WHAT'S CAUSING IT, BUT I DON'T CARE ANYMORE, FUCKASS INDEX OUT OF BOUNDS ERROR OH MY GOD MAN -mia
            }
        }, (Exception e) =>
        {
            Logger.LogError(e);
            Logger.LogError(e.ToString());
            Logger.LogError("Message: " + e.Message);
            Logger.LogError("StackTrace: " + e.StackTrace);
            Logger.LogError("Source: " + e.Source);
        });
    }

    public void OnPlayerSpawned()
    {
        // "There's probably a WAY better method of doing this but this is the only way I know how to do this.. :(" -mia
        foreach (GameObject rigObject in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(x => x.name == "Gorilla Player Networked(Clone)"))
        {
            Logger.LogInfo("Added nametag component to #" + rigObject.GetInstanceID());
            rigObject.AddComponent<Nametag>();
        }
    }
}
