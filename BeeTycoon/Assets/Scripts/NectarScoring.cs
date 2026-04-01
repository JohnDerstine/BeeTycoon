using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
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
    AudioSource source;

    PlayerController player;
    private HexMenu hexMenu;

    public Dictionary<FlowerType, float> nectarGains = new Dictionary<FlowerType, float>();
    public int populatedHives;

    private int totalAmountGained;
    private int flowerAmountGained;
    private bool calced;

    [SerializeField]
    private VisualTreeAsset nectarItem;

    [SerializeField]
    private Texture2D honeySprite;

    TemplateContainer item = null;
    VisualElement total;
    Label totalAmount;

    const int cloverValue = 10;
    const int alfalfaValue = 20;
    const int buckwheatValue = 15;
    const int fireweedValue = 30;
    const int goldenrodValue = 50;
    const int dandelionValue = 20;
    const int sunflowerValue = 10;
    const int orangeValue = 50;
    const int daisyValue = 50;
    const int thistleValue = 0;
    const int blueberryValue = 180;
    const int tupeloValue = 80;

    float basePitch;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void GameStart()
    {
        hexMenu = GameObject.Find("UIDocument").GetComponent<HexMenu>();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            nectarGains.Add(fType, 0);
        }

        basePitch = source.pitch;
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

    public IEnumerator GetNectarGains()
    {
        source.clip = audio;

        //Reset all values to 0
        ResetNectarGains();

        total = document.rootVisualElement.Q<VisualElement>("Total");
        totalAmount = total.Q<Label>("Amount");

        for (int i = 0; i < populatedHives; i++)
        {
            float duration = 0.5f;
            source.pitch = basePitch;
            player.hives[i].DisplayHiveRadius();
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
                }

                if (t.Flower != FlowerType.Empty)
                {
                    yield return new WaitWhile(() => !calced);
                    calced = false;
                    duration = DurationCalc(duration);
                }

            }
            yield return new WaitForSeconds(0.1f);
            player.hives[i].HideHiveRadius();
        }

        game.nectarCollectingFinished = true;

        populatedHives = 0;
        foreach (Hive h in player.hives)
            if (!h.queen.nullQueen)
                populatedHives++;

        yield return new WaitForSeconds(0.25f);
        yield break;
    }

    private void ResetNectarGains()
    {
        totalAmountGained = 0;
        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            nectarGains[fType] = 0;
        }
    }

    private void UpdateNectarUI(int spriteIndex)
    {
        item = nectarItem.Instantiate();
        item.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.allFlowerSprites[spriteIndex];
        document.rootVisualElement.Q<VisualElement>("NectarColumn").Insert(0, item);
    }

    private int ApplyModifierValues(FlowerType flower, List<Tile> adjTiles, List<Tile> diagTiles, int currentGain)
    {
        int newGain = currentGain;
        float mult = 1f;
        foreach (FlowerModifier m in mods.GetArchetypeAccquired<FlowerModifier>()) //Clover modifiers
        {
            if (m.Flowers[0] != FlowerType.Clover)
                break;

            int amountCheck = 0;
            if (m.Direction.Contains("adjacent"))
                foreach (Tile t in adjTiles)
                    if (t.Flower == m.Flowers[1])
                        amountCheck++;

            if (m.Direction.Contains("diagonal"))
                foreach (Tile t in diagTiles)
                    if (t.Flower == m.Flowers[1])
                        amountCheck++;

            if (amountCheck >= m.Amount)
            {
                if (m.BaseMod != 0)
                    newGain += m.BaseMod;
                else
                    mult = m.MultMod;
            }
        }
        return (int)(newGain * mult);
    }

    private void FlowerValueHelper(Tile t, int gain, float duration)
    {
        t.lastGain = gain;
        popUp.DisplayPopup(t.transform.position, gain, duration);
        flowerAmountGained += gain;
        totalAmountGained += gain;
        if (flowerAmountGained > 999)
            item.Q<Label>("Amount").style.fontSize = 48;
        if (totalAmountGained > 999)
            totalAmount.style.fontSize = 48;
        totalAmount.text = totalAmountGained.ToString();
        item.Q<Label>("Amount").text = flowerAmountGained.ToString();
    }

    private IEnumerator GetCloverValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(0);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        List<Tile> adjClover = map.GetAdjacentFlowers(FlowerType.Clover, t.x, t.y);
        List<Tile> diagClover = map.GetDiagonalFlowers(FlowerType.Clover, t.x, t.y);

        count += adjClover.Count;
        count += diagClover.Count;
        //Print tempCount to the screen above tile.
        //Animate flower
        StartCoroutine(t.Animate(FlowerType.Clover, 1, duration, true, source, h));
        int gain = (adjClover.Count + diagClover.Count) * cloverValue;
        gain = ApplyModifierValues(FlowerType.Clover, adjTiles, diagTiles, gain);

        FlowerValueHelper(t, gain, duration);

        //Animate related flowers
        foreach (Tile adjT in adjClover)
        {
            StartCoroutine(adjT.Animate(FlowerType.Clover, 0.3f, duration, false, source, h));
            //source.Play();
        }
        foreach (Tile diagT in diagClover)
        {
            StartCoroutine(diagT.Animate(FlowerType.Clover, 0.3f, duration, false, source, h));
            //source.Play();
        }

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        foreach (Tile adjT in adjClover)
            adjT.completed = false;
        foreach (Tile diagT in diagClover)
            diagT.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Clover] = count * cloverValue;
        flowerAmountGained = 0;
        calced = true;
        item = null;
    }

    private IEnumerator GetAlfalfaValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(1);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        List<Tile> diagAlfalfa = map.GetDiagonalFlowers(FlowerType.Alfalfa, t.x, t.y);
        count += diagAlfalfa.Count;

        StartCoroutine(t.Animate(FlowerType.Alfalfa, 1, duration, true, source, h));
        int gain = diagAlfalfa.Count * alfalfaValue;
        gain = ApplyModifierValues(FlowerType.Alfalfa, adjTiles, diagTiles, gain);

        FlowerValueHelper(t, gain, duration);

        //Animate related flowers
        foreach (Tile tDiag in diagAlfalfa)
            StartCoroutine(tDiag.Animate(FlowerType.Alfalfa, 0.3f, duration, false, source, h));

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        foreach (Tile tDiag in diagAlfalfa)
            tDiag.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Alfalfa] = count * alfalfaValue;
        flowerAmountGained = 0;
        calced = true;
        item = null;
    }

    private IEnumerator GetBuckwheatValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(2);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        count += buckwheatValue;
        StartCoroutine(t.Animate(FlowerType.Buckwheat, 1, duration, true, source, h));
        int gain = buckwheatValue;
        gain = ApplyModifierValues(FlowerType.Buckwheat, adjTiles, diagTiles, gain);

        FlowerValueHelper(t, gain, duration);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Buckwheat] = count;
        flowerAmountGained = 0;
        calced = true;
        item = null;
    }

    private IEnumerator GetFireweedValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(4);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        count += fireweedValue;
        StartCoroutine(t.Animate(FlowerType.Fireweed, 1, duration, true, source, h));
        int gain = fireweedValue;
        gain = ApplyModifierValues(FlowerType.Fireweed, adjTiles, diagTiles, gain);

        FlowerValueHelper(t, gain, duration);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Fireweed] = count;
        flowerAmountGained = 0;
        calced = true;
        item = null;
    }

    private IEnumerator GetGoldenrodValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(3);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        count += goldenrodValue;
        StartCoroutine(t.Animate(FlowerType.Goldenrod, 1, duration, true, source, h));
        int gain = goldenrodValue;
        gain = ApplyModifierValues(FlowerType.Goldenrod, adjTiles, diagTiles, gain);

        FlowerValueHelper(t, gain, duration);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Goldenrod] = count;
        calced = true;
        item = null;
    }

    private IEnumerator GetDandelionValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(5);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        count += dandelionValue;
        StartCoroutine(t.Animate(FlowerType.Dandelion, 1, duration, true, source, h));
        int gain = dandelionValue;
        gain = ApplyModifierValues(FlowerType.Dandelion, adjTiles, diagTiles, gain);

        FlowerValueHelper(t, gain, duration);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Dandelion] = count;
        flowerAmountGained = 0;
        calced = true;
        item = null;
    }

    private IEnumerator GetSunflowerValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(6);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        List<Tile> adjEmpty = map.GetAdjacentFlowers(FlowerType.Empty, t.x, t.y);
        List<Tile> diagEmpty = map.GetDiagonalFlowers(FlowerType.Empty, t.x, t.y);

        count += adjEmpty.Count;
        count += diagEmpty.Count;
        StartCoroutine(t.Animate(FlowerType.Sunflower, 1, duration, true, source, h));
        int gain = (adjEmpty.Count + diagEmpty.Count) * sunflowerValue;
        gain = ApplyModifierValues(FlowerType.Sunflower, adjTiles, diagTiles, gain);

        FlowerValueHelper(t, gain, duration);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Sunflower] = count * sunflowerValue;
        flowerAmountGained = 0;
        calced = true;
        item = null;
    }

    private IEnumerator GetOrangeValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(10);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        count += orangeValue;
        StartCoroutine(t.Animate(FlowerType.Orange, 1, duration, true, source, h));
        int gain = orangeValue;
        gain = ApplyModifierValues(FlowerType.Orange, adjTiles, diagTiles, gain);

        FlowerValueHelper(t, gain, duration);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Orange] = count;
        flowerAmountGained = 0;
        calced = true;
        item = null;
    }

    private IEnumerator GetDaisyValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(7);

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

        count += daisyValue * uniqueFlowers;
        StartCoroutine(t.Animate(FlowerType.Daisy, 1, duration, true, source, h));
        int gain = daisyValue * uniqueFlowers;
        gain = ApplyModifierValues(FlowerType.Daisy, adjTiles, diagTiles, gain);

        FlowerValueHelper(t, gain, duration);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Daisy] = count;
        flowerAmountGained = 0;
        calced = true;
        item = null;
    }

    private IEnumerator GetThistleValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(8);

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

        Tile randTile = validTiles[Random.Range(0, validTiles.Count)];

        count += randTile.lastGain * 3;
        StartCoroutine(t.Animate(FlowerType.Thistle, 1, duration, true, source, h));
        int gain = randTile.lastGain * 3;
        gain = ApplyModifierValues(FlowerType.Thistle, adjTiles, diagTiles, gain);

        FlowerValueHelper(t, gain, duration);

        randTile.lastGain = 0;
        randTile.Flower = FlowerType.Empty;

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Thistle] = count;
        flowerAmountGained = 0;
        calced = true;
        item = null;
    }

    private IEnumerator GetBlueberryValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(9);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        int gain = 0;
        if (game.Season == "summer")
        {
            count += blueberryValue;
            gain = blueberryValue;
        }
        gain = ApplyModifierValues(FlowerType.Blueberry, adjTiles, diagTiles, gain);

        StartCoroutine(t.Animate(FlowerType.Blueberry, 1, duration, true, source, h));

        FlowerValueHelper(t, gain, duration);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Blueberry] = count;
        flowerAmountGained = 0;
        calced = true;
        item = null;
    }

    private IEnumerator GetTupeloValue(Tile t, float duration, Hive h)
    {
        int count = 0;
        if (item == null)
            UpdateNectarUI(11);

        List<Tile> adjTiles = map.GetAdjacentTiles(t.x, t.y);
        List<Tile> diagTiles = map.GetDiagonalTiles(t.x, t.y);

        count += tupeloValue;
        StartCoroutine(t.Animate(FlowerType.Tupelo, 1, duration, true, source, h));
        int gain = tupeloValue;
        gain = ApplyModifierValues(FlowerType.Tupelo, adjTiles, diagTiles, gain);

        FlowerValueHelper(t, gain, duration);

        yield return new WaitWhile(() => !t.completed);
        t.completed = false;
        source.pitch += 0.25f;
        if (source.pitch >= 3)
            source.pitch = 3;
        nectarGains[FlowerType.Tupelo] = count;
        flowerAmountGained = 0;
        calced = true;
        item = null;
    }

}
