using System.Buffers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;

public class NectarScoring : MonoBehaviour
{
    [SerializeField]
    UIDocument document;

    [SerializeField]
    GameController game;

    [SerializeField]
    MapLoader map;

    [SerializeField]
    private RunModifiers mods;

    [SerializeField]
    private ResourcePopup popUp;

    [SerializeField]
    private AudioClip audio;

    [SerializeField]
    AudioSource source;

    [SerializeField]
    private VisualTreeAsset modIcon;

    PlayerController player;
    private HexMenu hexMenu;
    public int populatedHives;

    private List<int> usedSprites = new List<int>();

    private int totalAmountGained;
    private bool calced;

    [SerializeField]
    private VisualTreeAsset nectarItem;

    [SerializeField]
    private Texture2D honeySprite;

    TemplateContainer item = null;
    VisualElement total;
    Label totalAmount;

    Dictionary<FlowerType, int> gainValues = new Dictionary<FlowerType, int>()
    {
        {FlowerType.Clover, 10},
        {FlowerType.Alfalfa, 20},
        {FlowerType.Buckwheat, 10},
        {FlowerType.Goldenrod, 50},
        {FlowerType.Fireweed, 30},
        {FlowerType.Dandelion, 10},
        {FlowerType.Sunflower, 5},
        {FlowerType.Daisy, 50},
        {FlowerType.Thistle, 0},
        {FlowerType.Blueberry, 180},
        {FlowerType.Orange, 50},
        {FlowerType.Tupelo, 80},
        {FlowerType.Tulip, 100},
        {FlowerType.TulipPoplar, 10},
        {FlowerType.Hydrangea, 40},
        {FlowerType.WaterLily, 100},
        {FlowerType.Sundew, 50},
        {FlowerType.PitcherPlant, 100},
        {FlowerType.Lavendar, 25},
        {FlowerType.Hibiscus, 20},
    };

    Dictionary<FlowerType, int> spriteIndecies = new Dictionary<FlowerType, int>()
    {
        {FlowerType.Clover, 0},
        {FlowerType.Alfalfa, 1},
        {FlowerType.Buckwheat, 2},
        {FlowerType.Goldenrod, 3},
        {FlowerType.Fireweed, 4},
        {FlowerType.Dandelion, 5},
        {FlowerType.Sunflower, 6},
        {FlowerType.Daisy, 7},
        {FlowerType.Thistle, 8},
        {FlowerType.Blueberry, 9},
        {FlowerType.Orange, 10},
        {FlowerType.Tupelo, 11},
        {FlowerType.Tulip, -1},
        {FlowerType.TulipPoplar, 12},
        {FlowerType.Hydrangea, 13},
        {FlowerType.WaterLily, 14},
        {FlowerType.Sundew, 15},
        {FlowerType.PitcherPlant, 16},
        {FlowerType.Lavendar, 17},
        {FlowerType.Hibiscus, 18},
    };

    public int dandelionScaling = 0;

    float basePitch;

    List<Modifier> appliedMods = new List<Modifier>();
    Dictionary<Modifier, VisualElement> modElems = new Dictionary<Modifier, VisualElement>();

    [SerializeField]
    VisualTreeAsset multiplierLabel;

    TemplateContainer multiplierContainer;

    public FlowerType tulipType = FlowerType.Empty;

    public void GameStart()
    {
        hexMenu = GameObject.Find("HexMenu").GetComponent<HexMenu>();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        basePitch = source.pitch;

        do
        {
            tulipType = (FlowerType)UnityEngine.Random.Range(2, 22);
        } while (tulipType == FlowerType.Tulip);
        Debug.Log(tulipType);
    }

    private float DurationCalc(float duration)
    {
        float newDuration = duration;
        if (duration >= 0.005f)
            newDuration = duration * 0.8f;
        else                
            newDuration = 0.005f;

        return newDuration;
    }

    private void AdjustPitch()
    {
        source.pitch += 0.05f;
        if (source.pitch >= 1.75f)
            source.pitch = 1.75f;
    }

    //private void DisplayHiveMultiplier(Hive h)
    //{
    //    VisualElement baseElem = document.rootVisualElement.Q<VisualElement>("Base");
    //    if (multiplierContainer != null && baseElem.Contains(multiplierContainer))
    //    {
    //        baseElem.Remove(multiplierContainer);
    //        multiplierContainer = null;
    //    }

    //    multiplierContainer = multiplierLabel.Instantiate();
    //    multiplierContainer.Q<Label>().text = h.GetNectarMultiplier();

    //    baseElem.Add(multiplierContainer);

    //}

    public IEnumerator GetNectarGains()
    {
        source.clip = audio;
        total = document.rootVisualElement.Q<VisualElement>("Total");
        totalAmount = total.Q<Label>("Amount");

        //Display modifier icons for flower modifiers on left side of screen
        foreach (Modifier m in mods.accquiredMods)
        {
            if (m is FlowerModifier || m is OrderModifier)
            {
                TemplateContainer container = modIcon.Instantiate();
                VisualElement hex = container.Q<VisualElement>("Hex");
                VisualElement icon = container.Q<VisualElement>("Icon");
                hex.style.width = 256;
                hex.style.height = 256;
                icon.style.width = 150;
                icon.style.height = 150;
                icon.style.backgroundImage = m.Sprite;
                document.rootVisualElement.Q<VisualElement>("Modifiers").Add(container);

                modElems.Add(m, container);
            }
        }

        for (int i = 0; i < populatedHives; i++)
        {
            float duration = 0.5f;
            source.pitch = basePitch;
            player.hives[i].ResetNectarGains();
            player.hives[i].DisplayHiveRadius();
            popUp.DisplayPercent(player.hives[i]);
            //DisplayHiveMultiplier(player.hives[i]);
            foreach (Tile t in player.hives[i].tileRadius)
            {
                switch (t.Flower)
                {
                    case FlowerType.Empty:
                        break;
                    case FlowerType.Clover:
                        StartCoroutine(GetCloverValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Alfalfa:
                        StartCoroutine(GetAlfalfaValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Buckwheat:
                        StartCoroutine(GetBuckwheatValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Fireweed:
                        StartCoroutine(GetFireweedValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Goldenrod:
                        StartCoroutine(GetGoldenrodValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Dandelion:
                        StartCoroutine(GetDandelionValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Sunflower:
                        StartCoroutine(GetSunflowerValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Orange:
                        StartCoroutine(GetOrangeValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Daisy:
                        StartCoroutine(GetDaisyValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Blueberry:
                        StartCoroutine(GetBlueberryValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Tupelo:
                        StartCoroutine(GetTupeloValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Thistle:
                        StartCoroutine(GetThistleValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Tulip:
                        StartCoroutine(GetTulipValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.TulipPoplar:
                        StartCoroutine(GetTulipPoplarValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Hydrangea:
                        StartCoroutine(GetHydrangeaValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.WaterLily:
                        StartCoroutine(GetWaterLilyValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Sundew:
                        StartCoroutine(GetSundewValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.PitcherPlant:
                        StartCoroutine(GetPitcherPlantValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Lavendar:
                        StartCoroutine(GetLavendarValue(t, duration, player.hives[i]));
                        break;
                    case FlowerType.Hibiscus:
                        StartCoroutine(GetHibiscusValue(t, duration, player.hives[i]));
                        break;
                }

                if (t.Flower != FlowerType.Empty)
                {
                    yield return new WaitWhile(() => !popUp.complete);
                    popUp.complete = false;

                    yield return new WaitWhile(() => !calced);
                    calced = false;
                    duration = DurationCalc(duration);
                }
            }
            yield return new WaitForSeconds(1.5f);//give time for globs to finish going to hive
            player.hives[i].HideHiveRadius();
            document.rootVisualElement.Q<VisualElement>("NectarColumn").Clear();
            document.rootVisualElement.Q<VisualElement>("Modifiers").Clear();
            usedSprites.Clear();
            modElems.Clear();
            totalAmountGained = 0;
        }

        game.nectarCollectingFinished = true;

        populatedHives = 0;
        foreach (Hive h in player.hives)
            if (!h.queen.nullQueen)
                populatedHives++;

        yield return new WaitForSeconds(0.25f);

        usedSprites.Clear();
        modElems.Clear();
    }

    private void UpdateNectarUI(int spriteIndex)
    {
        if (usedSprites.Contains(spriteIndex))
        {
            item = document.rootVisualElement.Q<VisualElement>("NectarColumn").Q<VisualElement>(spriteIndex.ToString()) as TemplateContainer;
            return;
        }

       usedSprites.Add(spriteIndex);
       item = nectarItem.Instantiate();
       item.name = spriteIndex.ToString();
       item.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.allFlowerSprites[spriteIndex];
       document.rootVisualElement.Q<VisualElement>("NectarColumn").Insert(0, item);
    }

    private void TrackModifierStats(float original, float updated, FlowerModifier mod)
    {
        mod.Stat1++;
        mod.Stat2 += updated - original;
    }

    private int ApplyModifierValues(FlowerType flower, List<Tile> adjTiles, List<Tile> diagTiles, int currentGain, Hive h, Tile t)
    {
        appliedMods.Clear();
        int newGain = currentGain;
        float mult = 1f;

        List<FlowerModifier> flowerModifiers = mods.GetArchetypeAccquired<FlowerModifier>();
        List<OrderModifier> orderModifiers = mods.GetArchetypeAccquired<OrderModifier>();
        List<Modifier> allAccquired = new List<Modifier>();
        foreach (FlowerModifier m in flowerModifiers)
            allAccquired.Add(m);
        foreach (OrderModifier m in orderModifiers)
            allAccquired.Add(m);

        foreach (Modifier m in allAccquired)
        {
            if (m is FlowerModifier)
            {
                FlowerModifier fm = (FlowerModifier)m;
                if (fm.Flowers[0] == flower)
                {
                    if (FlowerModHelper(fm, adjTiles, diagTiles) >= fm.Amount)
                    {
                        newGain += fm.BaseMod;
                        mult *= fm.MultMod;
                        appliedMods.Add(fm);
                    }
                }
            }

            if (m is OrderModifier)
            {
                OrderModifier om = (OrderModifier)m;
                if (om.MyAttribute == "" || mods.flowerAttributes[flower].Contains(om.MyAttribute))
                {
                    if (om.Tiles == -1)
                    {
                        int num = OrderModHelper(om, h.tileRadius, t);
                        if (om.BaseMod != 0)
                            newGain += om.BaseMod * num;
                        if (om.MultMod != 1)
                            mult *= om.MultMod * num;
                        appliedMods.Add(om);
                    }
                    else if (OrderModHelper(om, h.tileRadius, t) >= om.Tiles)
                    {
                        newGain += om.BaseMod;
                        mult *= om.MultMod;
                        appliedMods.Add(om);
                    }
                }
            }
            //TrackModifierStats(currentGain, newGain, m);
        }
        return (int)(newGain * mult);
    }

    private int FlowerModHelper(FlowerModifier m, List<Tile> adjTiles, List<Tile> diagTiles)
    {
        int amountCheck = 0;
        if (m.Direction.Contains("adjacent"))
            foreach (Tile t in adjTiles)
                if (t.Flower == m.Flowers[1])
                    amountCheck++;

        if (m.Direction.Contains("diagonal"))
            foreach (Tile t in diagTiles)
                if (t.Flower == m.Flowers[1])
                    amountCheck++;

        return amountCheck;
    }

    private int OrderModHelper(OrderModifier m, List<Tile> hiveTiles, Tile tile)
    {
        int amountCheck = 0;

        if (m.Inf)
        {
            if (m.Before)
                foreach (Tile t in hiveTiles)
                    if (mods.flowerAttributes[t.Flower].Contains(m.Attribute) && hiveTiles.IndexOf(tile) > hiveTiles.IndexOf(t))
                        amountCheck++;

            if (m.After)
                foreach (Tile t in hiveTiles)
                    if (mods.flowerAttributes[t.Flower].Contains(m.Attribute) && hiveTiles.IndexOf(tile) < hiveTiles.IndexOf(t))
                        amountCheck++;
        }
        else
        {
            int startIndex = hiveTiles.IndexOf(tile);
            if (m.Before)
            {
                FlowerType stored = FlowerType.Wildflower;
                for (int i = startIndex + 1; i <= startIndex + m.Tiles; i++)
                {
                    if (i > hiveTiles.Count - 1)
                        break;

                    if (hiveTiles[i].Flower == FlowerType.Empty)
                    {
                        startIndex++;
                    }
                    else
                    {
                        if (!m.IsFlower)
                        {
                            if (mods.flowerAttributes[hiveTiles[i].Flower].Contains(m.Attribute))
                                amountCheck++;
                        }
                        else
                        {
                            if (stored == FlowerType.Wildflower && hiveTiles[i].Flower != FlowerType.Empty)
                                stored = hiveTiles[i].Flower;
                            if (stored == hiveTiles[i].Flower)
                                amountCheck++;
                        }
                    }
                }
            }
            else if (m.After)
            {
                FlowerType stored = FlowerType.Wildflower;
                for (int i = startIndex - 1; i >= startIndex - m.Tiles; i--)
                {
                    if (i < 0)
                        break;

                    if (hiveTiles[i].Flower == FlowerType.Empty)
                    {
                        startIndex--;
                    }
                    else
                    {
                        if (!m.IsFlower)
                        {
                            if (mods.flowerAttributes[hiveTiles[i].Flower].Contains(m.Attribute))
                                amountCheck++;
                        }
                        else
                        {
                            if (stored == FlowerType.Wildflower && hiveTiles[i].Flower != FlowerType.Empty)
                                stored = hiveTiles[i].Flower;
                            if (stored == hiveTiles[i].Flower)
                                amountCheck++;
                        }
                    }
                }
            }
        }
            return amountCheck;
    }

    private void FlowerValueHelper(Tile t, int gain, float duration, FlowerType f, Hive h)
    {
        t.lastGain = gain;
        for (int i = 0; i < appliedMods.Count; i++)
            StartCoroutine(popUp.ShakeModifier(modElems[appliedMods[i]], duration));

        StartCoroutine(popUp.AnimateNectar(gain, h, t.transform.position, duration));

        if (map.GetAdjacentTiles(h.x, h.y).Contains(t) || map.GetDiagonalTiles(h.x, h.y).Contains(t))
           h.buckfastGains[t.Flower] += gain;

        h.personalNectarGains[f] += gain;
        totalAmountGained += gain;
        if (h.personalNectarGains[f] > 999)
            item.Q<Label>("Amount").style.fontSize = 24;
        if (totalAmountGained > 999)
            totalAmount.style.fontSize = 24;
        totalAmount.text = Math.Round(totalAmountGained *.01f, 2, MidpointRounding.AwayFromZero).ToString() + " lbs.";
        item.Q<Label>("Amount").text = Math.Round(h.personalNectarGains[f] * .01f, 2, MidpointRounding.AwayFromZero).ToString() + " lbs.";
    }

    private bool CheckCaucasianCondition(Hive h)
    {
        int count = 0;
        foreach (Tile t in h.tileRadius)
            if (t.Flower == FlowerType.Empty)
                count++;
        if (count > Mathf.CeilToInt(h.tileRadius.Count / 2))
            return true;
        return false;
    }

    private int CalcHiveSharing(Tile t, int gain, Hive h)
    {
        //Before calcing hive sharing, check for Caucasian effect and Killer effect
        foreach (Hive h2 in player.hives)
        {
            if (h2.tileRadius.Contains(t) && h2.queen.species == "Caucasian" && CheckCaucasianCondition(h2))
                gain *= 2;
        }

        if (h.queen.species == "Killer")
            gain = (int)(gain * (h.StressLevel * 0.5f));

        bool russianPresent = false;
        int shareCount = 0;
        foreach (Hive hive in player.hives)
        {
            if (hive != h)
            {
                if (hive.tileRadius.Contains(t))
                    shareCount++;
                if (hive.queen.species == "Russian")
                    russianPresent = true;
            }
        }

        if (h.queen.species == "Russian" && russianPresent) //Might have to change this in case russian stacking is too effective
            return Mathf.FloorToInt(gain + (0.5f * gain * shareCount));
        else if (russianPresent)
            return 0;

        return Mathf.FloorToInt((gain + (0.5f * gain * shareCount)) / (shareCount + 1));
    }

    private IEnumerator GetCloverValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Clover;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        List<Tile> adjClover = map.GetAdjacentFlowers(FlowerType.Clover, t.x, t.y);
        List<Tile> diagClover = map.GetDiagonalFlowers(FlowerType.Clover, t.x, t.y);

        //Animate flower
        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = (adjClover.Count + diagClover.Count) * gainValues[flower];
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        //Animate related flowers
        foreach (Tile adjT in adjClover)
            StartCoroutine(adjT.Animate(flower, 0.3f, duration, false, source, h));
        foreach (Tile diagT in diagClover)
            StartCoroutine(diagT.Animate(flower, 0.3f, duration, false, source, h));

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        foreach (Tile adjT in adjClover)
            adjT.completed = false;
        foreach (Tile diagT in diagClover)
            diagT.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetAlfalfaValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Alfalfa;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        List<Tile> diagAlfalfa = map.GetDiagonalFlowers(flower, t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = diagAlfalfa.Count * gainValues[flower];
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        //Animate related flowers
        foreach (Tile tDiag in diagAlfalfa)
            StartCoroutine(tDiag.Animate(flower, 0.3f, duration, false, source, h));

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        foreach (Tile tDiag in diagAlfalfa)
            tDiag.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetBuckwheatValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Buckwheat;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower];
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetFireweedValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Fireweed;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower];
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetGoldenrodValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Goldenrod;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower];
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetDandelionValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Dandelion;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower] + dandelionScaling;
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetSunflowerValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Sunflower;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        List<Tile> adjEmpty = map.GetAdjacentFlowers(FlowerType.Empty, t.x, t.y);
        List<Tile> diagEmpty = map.GetDiagonalFlowers(FlowerType.Empty, t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = (adjEmpty.Count + diagEmpty.Count) * gainValues[flower];
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetOrangeValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Orange;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower];
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetDaisyValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Daisy;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        var fValues = System.Enum.GetValues(typeof(FlowerType));
        List<FlowerType> valueList = ((FlowerType[])fValues).ToList();
        List<Tile> validTiles = map.GetAdjacentTiles(t.x, t.y);
        foreach (Tile validT in map.GetDiagonalTiles(t.x, t.y))
            validTiles.Add(validT);

        int uniqueFlowers = 0;
        foreach (Tile validT in validTiles)
        {
            if (valueList.Contains(validT.Flower) && validT.Flower != FlowerType.Empty)
            {
                valueList.Remove(validT.Flower);
                uniqueFlowers++;
            }
        }

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower] * uniqueFlowers;
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetThistleValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Thistle;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        List<Tile> validTiles = map.GetAdjacentTiles(t.x, t.y);
        foreach (Tile validT in map.GetDiagonalTiles(t.x, t.y))
            validTiles.Add(validT);

        for (int k = 0; k < validTiles.Count; k++)
        {
            if (validTiles[k].Flower == FlowerType.Empty)
            {
                validTiles.RemoveAt(k);
                k--;
            }
        }

        Tile randTile = validTiles[UnityEngine.Random.Range(0, validTiles.Count)];

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = randTile.lastGain * 3;
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        randTile.lastGain = 0;
        randTile.Flower = FlowerType.Empty;

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetBlueberryValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Blueberry;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        int gain = 0;
        if (game.Season == "summer")
        {
            gain = gainValues[flower];
        }
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetTupeloValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Tupelo;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower];
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetTulipValue(Tile t, float duration, Hive h)
    {
        Debug.Log(tulipType);
        UpdateNectarUI(spriteIndecies[tulipType]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        int gain = gainValues[FlowerType.Tulip];
        gain = ApplyModifierValues(FlowerType.Tulip, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        StartCoroutine(t.Animate(tulipType, 1, duration, true, source, h));
        FlowerValueHelper(t, gain, duration, tulipType, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetTulipPoplarValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.TulipPoplar;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower];
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetHydrangeaValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.TulipPoplar;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower];

        bool water = false;
        foreach (Tile aT in adjTiles)
            if (aT.water)
                water = true;
        foreach (Tile dT in diagTiles)
            if (dT.water)
                water = true;
        if (water)
            gain *= 3;

        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetWaterLilyValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.WaterLily;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower];
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetSundewValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Sundew;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower];
        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetPitcherPlantValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.PitcherPlant;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        float distance = Mathf.Sqrt((float)Math.Pow(h.x - t.x, 2) + (float)Math.Pow(h.y - t.y, 2));
        int gain = Mathf.RoundToInt(gainValues[flower] * (1 / distance));

        int hiveCount = 1;
        foreach (Tile aT in adjTiles)
            if (aT.HasHive && aT.hive.queen != null)
                hiveCount++;
        gain *= hiveCount;

        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        h.population -= Mathf.RoundToInt(300 * (1 / distance));

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetLavendarValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Lavendar;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower];

        if (h.StressLevel >= 0)
            gain *= 2;
        else if (h.StressLevel < 0 && !h.conditions.Contains("Soothed"))
            h.AddCondition("Soothed");

        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }

    private IEnumerator GetHibiscusValue(Tile t, float duration, Hive h)
    {
        FlowerType flower = FlowerType.Sundew;
        UpdateNectarUI(spriteIndecies[flower]);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        StartCoroutine(t.Animate(flower, 1, duration, true, source, h));
        int gain = gainValues[flower] * h.conditions.Count();

        gain = ApplyModifierValues(flower, adjTiles, diagTiles, gain, h, t);
        gain = CalcHiveSharing(t, gain, h);
        FlowerValueHelper(t, gain, duration, flower, h);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        AdjustPitch();
        calced = true;
    }
}