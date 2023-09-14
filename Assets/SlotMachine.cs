using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SlotMachine : MonoBehaviour
{
    public Image slotMachineImage;
    public Button spinButton;
    public Sprite[] letterSprites;
    public float animationSpeed = 0.1f;

    private const string gatewayURL = "https://pas2-game-rd-lb.sayyogames.com:61337/api/unityexam/getroll";

    private bool isSpinning = false;
    private string[] currentRoll;

    private void Start()
    {
        // 设置拉霸机图像
        slotMachineImage.sprite = Resources.Load<Sprite>("slot_machine");

        // 设置按钮点击事件
        spinButton.onClick.AddListener(Spin);
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

        // 发送 POST 请求获取盘面结果
        yield return StartCoroutine(GetRollData());

        // 随机更换格子中的图片
        UpdateSlotMachine();

        isSpinning = false;
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

                // 解析 JSON 回应
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
        int slotCount = slotMachineImage.transform.childCount;

        for (int i = 0; i < slotCount; i++)
        {
            // 检查格子索引是否超出范围
            if (i >= currentRoll.Length)
                break;

            // 随机更换格子中的图片
            string letter = currentRoll[i];
            Image slotImage = slotMachineImage.transform.GetChild(i).GetComponent<Image>();
            slotImage.sprite = GetLetterSprite(letter);
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
