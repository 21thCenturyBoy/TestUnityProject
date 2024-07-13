namespace UnityEngine.UI
{
	/// <summary>
	/// 不规则图片
	/// </summary>
	[RequireComponent(typeof(PolygonCollider2D))]
	public class UIPolygonImage : Image
	{
		private PolygonCollider2D _polygon;

		protected UIPolygonImage()
		{
		}

        protected override void Start()
        {
	        GetComponent<Button>().onClick.AddListener(() =>
            {
	            Debug.LogError("onClick");
            });
        }

        private PolygonCollider2D Polygon
		{
			get
			{
				if (_polygon == null) _polygon = GetComponent<PolygonCollider2D>();
				return _polygon;
			}
		}

		public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
		{
			RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, eventCamera, out var point);

            return Polygon.OverlapPoint(point);
		}
		
#if UNITY_EDITOR
        protected override void Reset()
		{
			base.Reset();

			var w = rectTransform.sizeDelta.x * 0.5f + 0.1f;
			var h = rectTransform.sizeDelta.y * 0.5f + 0.1f;
			Polygon.points = new[]
			{
				new Vector2(-w, -h),
				new Vector2(w, -h),
				new Vector2(w, h),
				new Vector2(-w, h)
			};
		}
#endif
	}
}