using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Controller : MonoBehaviour
{
    public void Btn_SinglePlayer() { SC_Logic.Instance.Btn_SinglePlayerLogic(); }
    
    public void Btn_Multiplayer() { SC_Logic.Instance.Btn_MultiplayerLogic(); }

    public void Btn_StudentInfo() { SC_Logic.Instance.Btn_StudentInfoLogic(); }

    public void Btn_Settings() { SC_Logic.Instance.Btn_SettingsLogic(); }

    public void Btn_Back() { SC_Logic.Instance.Btn_BackLogic(); }

    public void Slider_Multiplayer() { SC_Logic.Instance.Slider_MultiplayerLogic(); }

    public void Slider_MusicVolume() { SC_Logic.Instance.Slider_MusicVolumeLogic(); }

    public void Slider_SfxVolume() { SC_Logic.Instance.Slider_SfxVolumeLogic(); }

    public void Btn_StartMultiplayer() { SC_Logic.Instance.Btn_StartMultiplayerLogic(); }

    public void Btn_CV() { SC_Logic.Instance.Btn_CVLogic(); }

    public void Btn_BackToMenu() { SC_Logic.Instance.Btn_BackToMenu(); }
}
