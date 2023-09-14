using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Button spinButton;
    public GameObject slotMachinePrefab;
    public Sprite[] letterSprites;

    private const string gatewayURL = "https://pas2-game-rd-lb.sayyogames.com:61337/api/unityexam/getroll";

    private bool isSpinning = false;
    private string[] currentRoll;
    private Sprite[] defaultLetterSprites; // 預設的字母圖片

    private void Start()
    {
        CreateSlotMachine();
    }

    private void CreateSlotMachine()
    {
        GameObject slotMachineObj = Instantiate(slotMachinePrefab, Vector3.zero, Quaternion.identity);
        slotMachineObj.transform.SetParent(transform);
        slotMachineObj.transform.localPosition = Vector3.zero;
        slotMachineObj.transform.localScale = Vector3.one;

        Slot[] slots = slotMachineObj.GetComponentsInChildren<Slot>();
        defaultLetterSprites = new Sprite[slots.Length]; // 初始化預設字母圖片陣列

        for (int i = 0; i < slots.Length; i++)
        {
            defaultLetterSprites[i] = slots[i].GetComponent<Image>().sprite; // 儲存預設字母圖片
        }
    }

    private Sprite GetRandomLetterSprite()
    {
        int randomIndex = Random.Range(0, letterSprites.Length);
        return letterSprites[randomIndex];
    }

    public void Spin()
    {
        if (isSpinning)
            return;

        StartCoroutine(SpinCoroutine());
    }

    private IEnumerator SpinCoroutine()
    {
        isSpinning = true;
        spinButton.interactable = false;

        yield return StartCoroutine(GetRollData());

        UpdateSlotMachine();

        isSpinning = false;
        spinButton.interactable = true;
    }

    private IEnumerator GetRollData()
    {
        WWWForm form = new WWWForm();
        form.AddField("METHOD", "spin");
        form.AddField("PARAMS", "test");

        using (UnityWebRequest www = UnityWebRequest.Post(gatewayURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResult = www.downloadHandler.text;
                ResponseData response = JsonUtility.FromJson<ResponseData>(jsonResult);
                int status = response.STATUS;

                if (status == 1)
                {
                    currentRoll = response.CURRENT_ROLL;
                }
                else
                {
                    string errorMsg = response.MSG;
                    Debug.LogError("Error: " + errorMsg);
                }
            }
            else
            {
                Debug.LogError("Network error: " + www.error);
            }
        }
    }

    private void UpdateSlotMachine()
    {
        GameObject slotMachineObj = GameObject.FindGameObjectWithTag("SlotMachine");
        if (slotMachineObj != null)
        {
            Slot[] slots = slotMachineObj.GetComponentsInChildren<Slot>();

            for (int i = 0; i < slots.Length; i++)
            {
                string letter = currentRoll[i];

                // 如果盤面字母為空，則顯示預設字母圖片
                if (string.IsNullOrEmpty(letter))
                {
                    slots[i].GetComponent<Image>().sprite = defaultLetterSprites[i];
                }
                else
                {
                    slots[i].SetLetterSprite(GetLetterSprite(letter));
                }
            }
        }
    }

    private Sprite GetLetterSprite(string letter)
    {
        int index = letter[0] - 'a';
        return letterSprites[index];
    }

    [System.Serializable]
    private class ResponseData
    {
        public int STATUS;
        public string MSG;
        public string[] CURRENT_ROLL;
    }
}
