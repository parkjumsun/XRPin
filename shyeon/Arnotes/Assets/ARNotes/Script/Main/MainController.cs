using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class MainController : MonoBehaviour
{
    public HandEnum handEnum;
    public PinManager pinManager;
    public JsonManager jsonManager; 
    public GameObject createUserInterface;
    public GameObject readUserInterface;
    public InterfaceToggle toggle;
    public GameObject indexTip;

    private MenuHover menuHover = new MenuHover();
    private HandGesture prevGesture;
    private List<Pin> currentLoadedPin = new List<Pin>();

    private bool pinGenerationMode = true;



    void Start()
    {

        this.currentLoadedPin = this.jsonManager.LoadAll();
        for(int i = 0; i < this.currentLoadedPin.Count;i++)
        {
            pinManager.DisplayPin(this.currentLoadedPin[i]);
        }
        Debug.Log("currentLoadedPin : " + currentLoadedPin.Count);
    }

    void Update()
    {
        if (!NRInput.Hands.IsRunning) return;
        var handState = NRInput.Hands.GetHandState(handEnum);
        if (pinGenerationMode)
        {
            this.indexTip.transform.position = handState.GetJointPose(HandJointID.IndexTip).position;
            JudgePinGeneration(handState);
        }
        this.prevGesture = handState.currentGesture;
    }

    private void JudgePinGeneration(HandState handState)
    {
        HandGesture gesture = handState.currentGesture;
        if (this.prevGesture != HandGesture.Pinch && gesture == HandGesture.Pinch)
        {
            this.DisablePinGenerationMode();
            Pin pin = pinManager.GeneratePin(this.indexTip.transform.position);
            this.pinManager.DisplayPin(pin);
            this.currentLoadedPin.Add(pin);
            this.UseCreatePanel();
            this.EnableCreateUserInterface();
            MenuHover.PassPin(pin);
        }
    }

    public void EnablePinGenerationMode()
    {
        this.pinGenerationMode = true;
        var handState = NRInput.Hands.GetHandState(handEnum);
        this.indexTip.transform.position = handState.GetJointPose(HandJointID.IndexTip).position;
        this.indexTip.SetActive(true);
    }

    public void DisablePinGenerationMode()
    {
        this.pinGenerationMode = false;
        this.indexTip.SetActive(false);
    }

    public void EnableCreateUserInterface()
    {
        toggle.InitializeCreatePanel();
    }

    public void DisableCreateUserInterface()
    {
        createUserInterface.SetActive(false);
    }
    public void EnableReadUserInterface()
    {
        toggle.InitializeReadPanel();
    }

    public void DisableReadUserInterface()
    {
        readUserInterface.SetActive(false);
    }

    public List<Pin> GetCurrentLoadedPin()
    {
        return this.currentLoadedPin;
    }

    public int GetCurrentNonCompletedPin()
    {
        int ret = 0;
        foreach(Pin pin in this.currentLoadedPin)
        {
            if (pin.GetPinStatus() != PinStatus.Completed) ret++;
        }
        return ret;
    }


    public Pin FindPinByName(string pinName)
    {
        foreach(Pin pin in currentLoadedPin)
        {
            if (pin.GetPinName() == pinName) return pin;
        }
        return null;
    }

    public bool IsEnableGenarationMode()
    {
        return this.pinGenerationMode;
    }

    public void UseCreatePanel()
    {
        toggle.UseCreatePanel();
    }

    public void ChangePinStatusIntoWorking(Pin pin)
    {
        if (pin.GetPinStatus() != PinStatus.Generated) return;
        pinManager.DestroyPInObject(pin);
        pinManager.ChangePinStatusIntoWorking(pin);
        pinManager.DisplayPin(pin);
        jsonManager.Save(pin);
    }
    public void ChangePinStatusIntoCompleted(Pin pin)
    {
        if (pin.GetPinStatus() != PinStatus.Working) return;
        pinManager.DestroyPInObject(pin);
        pinManager.ChangePinStatusIntoCompleted(pin);
        pinManager.DisplayPin(pin);
        jsonManager.Save(pin);
    }

    public void DeleteCurrentPin(Pin pin)
    {
        currentLoadedPin.Remove(pin);
        pinManager.DestroyPInObject(pin);
    }

    public void DeletePin(Pin pin)
    {
        this.currentLoadedPin.Remove(pin);
        pinManager.DestroyPInObject(pin);
        jsonManager.DeleteJsonFile(pin.GetPinName());
    }
}
