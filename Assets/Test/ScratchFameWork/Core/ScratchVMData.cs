namespace ScratchFramework
{
    public class ScratchVMData
    {
        private bool _isInitialized;
        public ScratchVMData Parent { get; set; }
        
        protected virtual void OnInitialize()
        {
        }
        public virtual void ShowData()
        {
            if (!_isInitialized)
            {
                OnInitialize();
                _isInitialized = true;
            }
        }
        public virtual void ClearData()
        {
        }

        public T Ancestors<T>(ScratchVMData origin) where T : ScratchVMData
        {
            if (origin == null) return null;

            var parentViewModel = origin.Parent;
            while (parentViewModel != null)
            {
                if (parentViewModel is T castedViewModel)
                {
                    return castedViewModel;
                }
                parentViewModel = parentViewModel.Parent;
            }
            return null;
        }
    }
}

