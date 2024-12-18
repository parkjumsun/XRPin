using UnityEngine;
using UnityEngine.UI;
using NRKernal;
using Unity.VisualScripting;
using LitJson;

public class MenuHover : MonoBehaviour
{
    public MenuCommander menuCommander;
    public HandEnum handEnum;
    public RectTransform panelRectTransform;
    public Transform panel;
    public Image targetImage;
    public static Image currentImage;
    public float hoverTime = 0.5f;
    public float buttonHoverTime = 0.5f;
    public Camera nrealCamera;
    public GameObject[] thickButtons;
    public float speed = 1f;
    public Image SubTargetImage;

    public RecordHandler recordHandler;
    private bool isRecording = false;
    private Color originalTargetColor;
    private Color recordingColor = Color.red; // 녹화 중일 때의 색상
    private Vector3 initialSize = new Vector3(1.0f, 1.0f, 1.0f);
    private Vector3 scaledSize = new Vector3(1.2f, 1.2f, 1.2f);
    public SaveLineRenderer saveLineRenderer;
    public JsonManager jsonManager;
    public MainController mainController;

    private float hoverTimer = 0f;
    private bool isHovering = false;
    private GameObject currentHoveredButton = null;
    private float buttonHoverTimer = 0f;

    private static Pin currentPin;


    void Start()
    {
        if (SubTargetImage != null)
        {
            originalTargetColor = SubTargetImage.color;
        }
        HideThickButtons();
    }

    void Update()
    {
        if (!NRInput.Hands.IsRunning)
            return;

        var handState = NRInput.Hands.GetHandState(handEnum);
        var pose = handState.GetJointPose(HandJointID.IndexTip);

        Vector3 screenPoint = nrealCamera.WorldToScreenPoint(pose.position);
        Vector2 pointerScreenPos = new Vector2(screenPoint.x, screenPoint.y);

        if (targetImage.name != "record" && targetImage.name != "save" && targetImage.name != "cancel" && thickButtons.Length != 0 && thickButtons[0].activeSelf)
        {
            if (currentImage != null && currentImage != targetImage)
            {
                HideThickButtonsUnderImage(currentImage);
            }
            CheckHoverOnThickButtons(pointerScreenPos);
        }
        else if (thickButtons.Length == 0)
        {
            CheckNoHover(pointerScreenPos);
        }
        else
        {
            CheckHoverOnPanel(pointerScreenPos);
        }
    }
    

    void CheckHoverOnPanel(Vector2 pointerScreenPos)
    {
        targetImage.transform.localScale = Vector3.Lerp(initialSize, scaledSize, hoverTimer * 3f);
        isHovering = RectTransformUtility.RectangleContainsScreenPoint(panelRectTransform, pointerScreenPos, nrealCamera);

        if (RectTransformUtility.RectangleContainsScreenPoint(targetImage.rectTransform, pointerScreenPos, nrealCamera))
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= hoverTime)
            {
                Debug.Log(targetImage.transform.localScale);
                //if (currentImage != null && currentImage != targetImage)
                //{
                //    hoverTimer -= Time.deltaTime * 3f;
                //    if (hoverTimer <= 0) hoverTimer = 0;
                //    HideThickButtonsUnderImage(currentImage);
                //}
                //hoverTimer = 0;
                ShowThickButtons();
                if (currentImage == null)
                {
                    currentImage = targetImage;
                }
            }
        }
        else
        {
            hoverTimer = 0;
            HideThickButtons();
        }
    }

    void CheckNoHover(Vector2 pointerScreenPos)
    {
        targetImage.transform.localScale = Vector3.Lerp(initialSize, scaledSize, hoverTimer * 3f);
        if (RectTransformUtility.RectangleContainsScreenPoint(targetImage.rectTransform, pointerScreenPos, nrealCamera))
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= hoverTime)
            {
                if (currentImage == null)
                {
                    currentImage = targetImage;
                }
                switch (targetImage.name)
                {
                    case "record":
                        if (!isRecording)
                        {
                            //string recordVideoSavePath 
                            isRecording = true;
                            recordHandler.StartRecording();
                            string videoPath = recordHandler.recordButtonHandler.VideoSavePath;
                            Debug.Log(videoPath);
                            currentPin.setVideoPath(videoPath);
                            RecordingColor(SubTargetImage);
                            //빨간색으로 바꾸는 로직 추가
                            hoverTimer = 0;
                            this.mainController.DisableCreateUserInterface();
                        }
                        else if (isRecording)
                        {
                            isRecording = false;
                            recordHandler.StopRecording(); // 녹화 중지
                            //원래 색으로 바꾸는 로직 추가
                            ResetColor(SubTargetImage, originalTargetColor);
                            Debug.Log("녹화 중지");
                        }

                        // 한번 실행 후 일정 시간 대기
                        hoverTimer = 0;                    
                        Debug.Log("record 버튼 호출");
                        break;
                    case "save":
                        Debug.Log("##############################");
                        currentPin.SetMemos(saveLineRenderer.GetLineObject()); // JSON 데이터 가져오기
                        jsonManager.Save(currentPin);
                        menuCommander.Command(targetImage.name);
                        mainController.EnablePinGenerationMode();
                        mainController.DisableCreateUserInterface();
                        //Debug.Log("save 버튼 호출");
                        break;
                    case "cancel":
                        Debug.Log("cancel 버튼 호출");
                        menuCommander.Command(targetImage.name);
                        mainController.EnablePinGenerationMode();
                        mainController.DisableCreateUserInterface();
                        mainController.DeleteCurrentPin(currentPin);
                        break;
                }
            }
        }
        else
        {
            hoverTimer = 0;
        }
    }

    void CheckHoverOnThickButtons(Vector2 pointerScreenPos)
    {
        foreach (var button in thickButtons)
        {
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(buttonRect, pointerScreenPos, nrealCamera))
            {
                button.transform.localScale = Vector3.Lerp(initialSize, scaledSize, buttonHoverTimer * 2f);

                if (currentHoveredButton != button)
                {
                    currentHoveredButton = button;
                    buttonHoverTimer = 0f;
                }

                buttonHoverTimer += Time.deltaTime;

                if (buttonHoverTimer >= buttonHoverTime)
                {
                    menuCommander.Command(button.name);
                    HideThickButtons();
                }
                return;
            }
        }

        buttonHoverTimer = 0f;
        currentHoveredButton = null;
    }

    void ShowThickButtons()
    {
        InvertColor(SubTargetImage);

        foreach (var button in thickButtons)
        {
            button.SetActive(true);
        }
        Debug.Log("Thick buttons are now visible");
    }

    void HideThickButtons()
    {
        foreach (var button in thickButtons)
        {
            button.SetActive(false);
        }
        buttonHoverTimer = 0f;
        currentHoveredButton = null;
        isHovering = false;
        ResetColor(SubTargetImage, originalTargetColor);
    }

    void InvertColor(Image image)
    {
        if (image != null)
        {
            Color invertedColor = new Color(1 - image.color.r, 1 - image.color.g, 1 - image.color.b, image.color.a);
            image.color = invertedColor;
        }
    }

    void ResetColor(Image image, Color originalColor)
    {
        if (image != null)
        {
            image.color = originalColor;
        }
    }

    void RecordingColor (Image image)
    {
        if (image != null)
        {
            Color RecordingColor = new Color(1, 0, 0, image.color.a);
            image.color = RecordingColor;
        }
    }

    void HideThickButtonsUnderImage(Image parentImage)
    {
        currentImage = null;
        ResetColor(SubTargetImage, originalTargetColor);

        foreach (var button in thickButtons)
        {
            button.SetActive(false);
        }
    }

    public static void PassPin(Pin pin)
    {
        currentPin = pin;
        Debug.Log(pin.GetPinName());
    }
}