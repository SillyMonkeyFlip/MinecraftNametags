using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GorillaLibrary.Utilities;
using GorillaLibrary.Extensions;
using GorillaTagScripts;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Logging;

namespace MinecraftNametags.Behaviours;

public enum Significance
{
    VIM,
    Known,      //DIAMOND
    Developer,  //CMD
    AAC,
    Boyfriend,  //DYE
    Golden      //GOLD
}

/// <summary>
/// "Please, if any of this looks horrible to you (It definitely does), PLEASE PR this, I'm not a good coder by any means I'm just trying my best :("  -mia
/// "Also, please someone find a better way to sync the hearts in Paintbrawl, i've practically had to bruteforce the syncing for it to not break or whatever." -mia
/// </summary>
public class Nametag : MonoBehaviour
{
    public static List<Nametag> All = new();

    private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Nametag");

    public VRRig rig;

    public GameObject nametag;
    public Canvas canvas;

    public Sprite[] healthSprite;
    public Sprite[] significanceSprite;

    public GameObject speakerIcon;
    public Sprite regularSpeaker;
    public Sprite mutedSpeaker;

    public TextMeshProUGUI nameText;
    public Image outline;

    public RigContainer? rigContainer;

    public GameObject paintBrawlHealthParent;
    public GameObject[] paintbrawlHealth;

    public bool loaded;

    public Image significanceIcon;

    public void Awake()
    {
        All.Add(this);

        rig = GetComponent<VRRig>();
        Logger.LogInfo("Loading nametag object...");

        nametag = Instantiate(Plugin.Instance.Bundle.LoadAsset<GameObject>("Nametag"));
        healthSprite = Plugin.Instance.Bundle.LoadAssetWithSubAssets<Sprite>("hearts_sheet");
        significanceSprite = Plugin.Instance.Bundle.LoadAssetWithSubAssets<Sprite>("iconsheet");

        regularSpeaker = Plugin.Instance.Bundle.LoadAsset<Sprite>("voicechat_icon");
        mutedSpeaker = Plugin.Instance.Bundle.LoadAsset<Sprite>("voicemute_icon");

        OnLoadComplete();
    }

    public void OnEnable()
    {
        UpdateState();
        StartCoroutine(Tick());
    }

    IEnumerator Tick() //if all else fails resort to 1 second tick :( -mia
    {
        for (; ; )
        {
            if (!enabled)
                yield break; //???
            yield return new WaitForSeconds(1);
            UpdateState();
        }
    }

    public void OnLoadComplete()
    {
        nametag.transform.SetParent(rig.transform, false);
        canvas = nametag.transform.GetChild(0).GetComponent<Canvas>();

        speakerIcon = canvas.transform.Find("Speaker").gameObject;
        nameText = canvas.transform.Find("Nameplate").gameObject.GetComponent<TextMeshProUGUI>();
        outline = canvas.transform.Find("Outline").GetComponent<Image>();
        significanceIcon = canvas.transform.Find("Icon").GetComponent<Image>();

        //"this is done horribly ;-;" -mia
        paintBrawlHealthParent = canvas.transform.Find("Paintbrawl Health").gameObject;
        paintbrawlHealth =
        [
            paintBrawlHealthParent.transform.GetChild(0).gameObject,
            paintBrawlHealthParent.transform.GetChild(1).gameObject,
            paintBrawlHealthParent.transform.GetChild(2).gameObject,
        ];

        rig.OnColorChanged += color => { UpdateState(); };
        rig.OnNameChanged += container => { UpdateState(); };
        loaded = true;
        UpdateState();
    }

    public void OnDisable()
    {
        rigContainer = null;
    }

    public void Update()
    {
        if (!loaded) return;

        if (rigContainer)
        {
            if (!rigContainer.IsMuted)
            {
                if (speakerIcon.GetComponent<Image>().sprite != regularSpeaker)
                    speakerIcon.GetComponent<Image>().sprite = regularSpeaker;
            }
            else
            {
                if (speakerIcon.GetComponent<Image>().sprite != mutedSpeaker)
                    speakerIcon.GetComponent<Image>().sprite = mutedSpeaker;
            }

            speakerIcon.SetActive(rigContainer.IsMuted || rigContainer.Voice.IsSpeaking);
        }

        canvas.transform.eulerAngles = new Vector3(GorillaTagger.Instance.mainCamera.transform.eulerAngles.x,
            GorillaTagger.Instance.mainCamera.transform.eulerAngles.y, 0);

        nameText.text = rig.playerText1.text;
    }

    public static void UpdateAllPaintbrawl()
    {
        if (!PhotonNetwork.InRoom) return;
        if (GorillaGameManager.instance == null) return;
        if (GorillaGameManager.instance is not GorillaPaintbrawlManager) return;

        foreach (Nametag nametag in Nametag.All)
            nametag.UpdatePaintbrawlState();
    }

    public void UpdatePaintbrawlState()
    {
        if (paintBrawlHealthParent.activeSelf)
        {
            for (int i = 0; i < paintbrawlHealth.Length; i++)
            {
                var image = paintbrawlHealth[i].GetComponent<Image>();
                bool isActive = rig.paintbrawlBalloons.balloons[i].activeSelf;

                image.sprite = isActive ? healthSprite[0] : healthSprite[1];
            }
        }
    }

    public void UpdateState()
    {
        if (!loaded) return;
        if (GorillaGameManager.instance == null) return;
        if (rigContainer == null)
        {
            try
            {
                rigContainer = RigUtility.GetRig(rig.Creator);
            }
            catch
            {
                return;
            }
        }
        paintBrawlHealthParent.SetActive(GorillaGameManager.instance is GorillaPaintbrawlManager); //"Probably not the best way but it works :thinking:" -mia
        UpdatePaintbrawlState();

        outline.color = rig.playerColor;

        CheckSignificance(rig);
    }

    // i hate everything about these methods -mia

    public void SetSignificanceIcon(int index)
    {
        significanceIcon.sprite = significanceSprite[index];
        significanceIcon.gameObject.SetActive(true);
        significanceIcon.enabled = true;
    }

    public void CheckSignificance(VRRig rig)
    {
        if (rig._playerOwnedCosmetics.Contains("LBANI."))
        {
            if (rig.GetCosmetics().items.Any(x => x.itemName == "LBANI."))
            {
                SetSignificanceIcon(3); //AAC
                return;
            }
        }
        if (Plugin.Instance.SignificanceMapping.TryGetValue(rig.Creator.UserId, out Significance significance))
        {
            SetSignificanceIcon((int)significance);
            return;
        }
        if (SubscriptionManager.IsPlayerSubscribed(rig))
        {
            SetSignificanceIcon(0); //VIM
            return;
        }
        significanceIcon.gameObject.SetActive(false);
        significanceIcon.enabled = false;
    }
}
