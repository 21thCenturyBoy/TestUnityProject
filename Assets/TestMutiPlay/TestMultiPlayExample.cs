using System.Collections;
using System.Collections.Generic;
using TestMutiPlay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestMultiPlayExample : MonoBehaviour
{
    public Button StartMutltiButton;
    public Button CloseMutltiButton;

    public TMP_Text DataText;
    // Start is called before the first frame update
    void Start()
    {

        StartMutltiButton?.onClick.AddListener(() =>
        {
            MultiUserEditorDebuger.Instance.StudioProcessStart(1);
        });

        CloseMutltiButton?.onClick.AddListener(() =>
        {
            MultiUserEditorDebuger.Instance.KillAllStudioProcess();
        });

        DataText.text = JsonUtility.ToJson(MultiUserEditorDebuger.GetInstance().Data,true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
