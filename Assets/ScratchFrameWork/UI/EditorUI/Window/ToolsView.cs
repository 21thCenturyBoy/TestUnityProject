using UnityEngine;

namespace ScratchFramework
{
    public class ToolsView : BaseWindowsView
    {
        public IconButton CloseBtn => m_CloseBtn;
        public IconButton ObjectViewBtn => m_ObjectViewBtn;
        public IconButton SceneBtn => m_SceneBtn;
        public IconButton CodeBtn => m_CodeBtn;
        public IconButton MaterialsBtn => m_MaterialsBtn;
        public IconButton UndoBtn => m_UndoBtn;
        public IconButton RedoBtn => m_RedoBtn;
        public IconButton VariableBtn => m_VariableBtn;
        public IconButton ParamBtn => m_ParamBtn;
        public IconButton ToolsBtn => m_ToolsBtn;
        public IconButton PlayBtn => m_PlayBtn;
        public IconButton PasueBtn => m_PasueBtn;
        public IconButton StopBtn => m_StopBtn;


        private IconButton m_CloseBtn;
        private IconButton m_ObjectViewBtn;
        private IconButton m_SceneBtn;
        private IconButton m_CodeBtn;
        private IconButton m_MaterialsBtn;
        private IconButton m_UndoBtn;
        private IconButton m_RedoBtn;
        private IconButton m_VariableBtn;
        private IconButton m_ParamBtn;
        private IconButton m_ToolsBtn;
        private IconButton m_PlayBtn;
        private IconButton m_PasueBtn;
        private IconButton m_StopBtn;

        public override bool Initialize()
        {
            m_CloseBtn = transform.Find("ContentFrame/InnerFrame/View/Close").GetComponent<IconButton>();
            m_ObjectViewBtn = transform.Find("ContentFrame/InnerFrame/View/ObjectView").GetComponent<IconButton>();
            m_SceneBtn = transform.Find("ContentFrame/InnerFrame/View/Scene").GetComponent<IconButton>();
            m_CodeBtn = transform.Find("ContentFrame/InnerFrame/View/Code").GetComponent<IconButton>();
            m_MaterialsBtn = transform.Find("ContentFrame/InnerFrame/View/Materials").GetComponent<IconButton>();
            m_UndoBtn = transform.Find("ContentFrame/InnerFrame/View/Undo").GetComponent<IconButton>();
            m_RedoBtn = transform.Find("ContentFrame/InnerFrame/View/Redo").GetComponent<IconButton>();
            m_VariableBtn = transform.Find("ContentFrame/InnerFrame/View/Variable").GetComponent<IconButton>();
            m_ParamBtn = transform.Find("ContentFrame/InnerFrame/View/Param").GetComponent<IconButton>();
            m_ToolsBtn = transform.Find("ContentFrame/InnerFrame/View/Tools").GetComponent<IconButton>();
            m_PlayBtn = transform.Find("ContentFrame/InnerFrame/View/Play").GetComponent<IconButton>();
            m_PasueBtn = transform.Find("ContentFrame/InnerFrame/View/Pasue").GetComponent<IconButton>();
            m_StopBtn = transform.Find("ContentFrame/InnerFrame/View/Stop").GetComponent<IconButton>();

            m_CloseBtn.Initialize();
            m_ObjectViewBtn.Initialize();
            m_SceneBtn.Initialize();
            m_CodeBtn.Initialize();
            m_MaterialsBtn.Initialize();
            m_UndoBtn.Initialize();
            m_RedoBtn.Initialize();
            m_VariableBtn.Initialize();
            m_ParamBtn.Initialize();
            m_ToolsBtn.Initialize();
            m_PlayBtn.Initialize();
            m_PasueBtn.Initialize();
            m_StopBtn.Initialize();

            m_CloseBtn.RemoveAllListener();
            m_CloseBtn.AddListener(() => { ToolHelper.ExitScratchEditor(); });

            m_ObjectViewBtn.RemoveAllListener();
            m_ObjectViewBtn.AddListener(() => { ToolHelper.OpenObjectView(); });

            m_SceneBtn.RemoveAllListener();
            m_SceneBtn.AddListener(() => { ToolHelper.OpenSceneView(); });

            m_CodeBtn.RemoveAllListener();
            m_CodeBtn.AddListener(() => { ToolHelper.OpenCodeView(); });

            m_MaterialsBtn.RemoveAllListener();
            m_MaterialsBtn.AddListener(() => { ToolHelper.OpenMaterialView(); });

            m_UndoBtn.RemoveAllListener();
            m_UndoBtn.AddListener(() => { });

            m_RedoBtn.RemoveAllListener();
            m_RedoBtn.AddListener(() => { });

            m_VariableBtn.RemoveAllListener();
            m_VariableBtn.AddListener(() => { });

            m_ParamBtn.RemoveAllListener();
            m_ParamBtn.AddListener(() => { });

            m_ToolsBtn.RemoveAllListener();
            m_ToolsBtn.AddListener(() => { });

            m_PlayBtn.RemoveAllListener();
            m_PlayBtn.AddListener(ToolHelper.OnPlay);

            m_PasueBtn.RemoveAllListener();
            m_PasueBtn.AddListener(ToolHelper.OnPause);

            m_StopBtn.RemoveAllListener();
            m_StopBtn.AddListener(ToolHelper.OnStop);

            InitBtnState();

            return base.Initialize();
        }

        public void InitBtnState()
        {
            m_PlayBtn.Active = true;
            m_PasueBtn.Active = false;
        }
    }
}