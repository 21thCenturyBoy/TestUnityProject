using System;
using System.ComponentModel;
using System.IO;
using UnityEngine.UI;

namespace ScratchFramework
{
    [Serializable]
    public sealed class BlockHeaderParam_Data_Icon : BlockHeaderParam_Data<BlockHeaderParam_Data_Icon>
    {
        private string _iconResource = string.Empty;

        public string IconResourcePath
        {
            get => _iconResource;
            set
            {
                if (value == _iconResource) return;
                _iconResource = value;
                OnPropertyChanged();
            }
        }

        public override DataType DataType => DataType.Icon;

        protected override byte[] OnSerialize()
        {
            var bytes = ScratchUtils.ScratchSerializeString(IconResourcePath);
            return bytes;
        }

        protected override void OnDeserialize(MemoryStream memoryStream, int version = -1)
        {
            IconResourcePath = memoryStream.ScratchDeserializeString();
        }
    }

    public class BlockHeaderItem_Icon : BlockHeaderItem<BlockHeaderParam_Data_Icon>
    {
        private Image m_IconImage;

        public Image IconImage
        {
            get
            {
                if (m_IconImage == null)
                {
                    m_IconImage = GetComponent<Image>();
                }

                return m_IconImage;
            }
        }

        protected override void OnCreateContextData()
        {
            ContextData.IconResourcePath = IconImage.sprite.name;
        }


        public override void ContextDataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BlockHeaderParam_Data_Icon.IconResourcePath):
                    // TODO Load Icon
                    break;
                default:
                    break;
            }
        }

        public override void RefreshUI()
        {
            // TODO Load Icon
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(IconImage)}: {IconImage}";
        }
    }
}