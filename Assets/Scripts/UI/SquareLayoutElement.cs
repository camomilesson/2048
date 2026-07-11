using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SquareLayoutElement :
        MonoBehaviour,
        ILayoutElement
    {
        private RectTransform cachedRectTransform;

        private RectTransform RectTransform
        {
            get
            {
                if (cachedRectTransform == null)
                {
                    cachedRectTransform =
                        GetComponent<RectTransform>();
                }

                return cachedRectTransform;
            }
        }

        /*
         * Do not request a particular width.
         * MainScreen's Vertical Layout Group controls it.
         */
        public float minWidth
        {
            get
            {
                return -1f;
            }
        }

        public float preferredWidth
        {
            get
            {
                return -1f;
            }
        }

        public float flexibleWidth
        {
            get
            {
                return -1f;
            }
        }

        /*
         * Once the parent has assigned our width,
         * request the same amount of height.
         */
        public float minHeight
        {
            get
            {
                return RectTransform.rect.width;
            }
        }

        public float preferredHeight
        {
            get
            {
                return RectTransform.rect.width;
            }
        }

        public float flexibleHeight
        {
            get
            {
                return 0f;
            }
        }

        public int layoutPriority
        {
            get
            {
                return 1;
            }
        }

        public void CalculateLayoutInputHorizontal()
        {
            // Width is determined by the parent layout group.
        }

        public void CalculateLayoutInputVertical()
        {
            // Height properties read the width assigned
            // during the horizontal layout pass.
        }

        private void OnEnable()
        {
            MarkLayoutForRebuild();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            MarkLayoutForRebuild();
        }

        private void OnValidate()
        {
            MarkLayoutForRebuild();
        }

        private void MarkLayoutForRebuild()
        {
            if (RectTransform.parent is RectTransform parentRect)
            {
                LayoutRebuilder.MarkLayoutForRebuild(
                    parentRect
                );
            }
        }
    }
}