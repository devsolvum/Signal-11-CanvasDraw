using System;
using System.Collections.Generic;
using System.Threading;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Math = Java.Lang.Math;
using Object = Java.Lang.Object;

namespace App1.Domain
{
	// https://medium.com/@fanfatal/android-swipe-menu-with-recyclerview-8f28a235ff28
	// https://github.com/FanFataL/swipe-controller-demo/blob/master/app/src/main/java/pl/fanfatal/swipecontrollerdemo/SwipeController.java

	public enum SwipeState
	{
		Default,
		LeftOpen,
		RightOpen
	}

	public class LeftRightSwipeController : ItemTouchHelper.Callback
	{
		private bool _swipeBack = false;

		private SwipeState _currentSwipeState = SwipeState.Default;

		private RectF _buttonInstance = null;

		public override int ConvertToAbsoluteDirection(int flags, int layoutDirection)
		{
			if (_swipeBack)
			{
				_swipeBack = _currentSwipeState != SwipeState.Default;
				return 0;
			}
			return base.ConvertToAbsoluteDirection(flags, layoutDirection);
		}

		public override void OnChildDraw(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX,
			float dY, int actionState, bool isCurrentlyActive)
		{
			var measuredsize = GetMeasuredSize(GetContainerArea(dX), viewHolder);
			if (measuredsize.width <= 0 || measuredsize.height <= 0)
				return;

			var clippedX = dX >= 0 ? Math.Min(dX, measuredsize.width) : Math.Max(dX, -measuredsize.width);

			if (actionState == ItemTouchHelper.ActionStateSwipe)
			{
				if (_currentSwipeState != SwipeState.Default)
				{
					if (_currentSwipeState == SwipeState.LeftOpen)
						clippedX = Math.Max(clippedX, measuredsize.width);
					if (_currentSwipeState == SwipeState.RightOpen)
						clippedX = Math.Min(clippedX, -measuredsize.width);

					base.OnChildDraw(c, recyclerView, viewHolder, clippedX, dY, actionState, isCurrentlyActive);
				}
				else
				{
					SetTouchListener(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
				}
			}

			if (_currentSwipeState == SwipeState.Default)
			{
				base.OnChildDraw(c, recyclerView, viewHolder, clippedX, dY, actionState, isCurrentlyActive);
			}

			DrawButtons(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive, _currentSwipeState, false);
		}

		private (int width, int height) GetMeasuredSize(int containerArea, RecyclerView.ViewHolder viewHolder)
		{
			if (viewHolder is IViewHolderButtonContainer buttonContainer)
			{
				int widthSpec = View.MeasureSpec.MakeMeasureSpec(viewHolder.ItemView.Width, MeasureSpecMode.AtMost);
				int heightSpec = View.MeasureSpec.MakeMeasureSpec(viewHolder.ItemView.Height, MeasureSpecMode.AtMost);

				var container = buttonContainer.CreateButtonContainer(containerArea);
				container.Measure(widthSpec, heightSpec);

				return (container.MeasuredWidth, container.MeasuredHeight);
			}

			return (0, 0);
		}

		private void SetItemsClickable(RecyclerView recyclerView, bool isClickable)
		{
			for (int i = 0; i < recyclerView.ChildCount; ++i)
			{
				recyclerView.GetChildAt(i).Clickable = isClickable;
			}
		}

		private void SetTouchListener(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX,
			float dY, int actionState, bool isCurrentlyActive)
		{
			recyclerView.SetOnTouchListener(
				new TouchListenerDelegate(
					(v, @event) =>
					{
						_swipeBack = @event.Action == MotionEventActions.Cancel || @event.Action == MotionEventActions.Up;
						if (_swipeBack)
						{
							float containerWidth = GetMeasuredSize(GetContainerArea(dX), viewHolder).width;

							if (dX <= -containerWidth)
							{
								_currentSwipeState = SwipeState.RightOpen;
							}
							else if (dX >= containerWidth)
							{
								_currentSwipeState = SwipeState.LeftOpen;
							}

							if (_currentSwipeState != SwipeState.Default)
							{
								DrawButtons(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive, _currentSwipeState, true);
								SetTouchDownListener(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
								SetItemsClickable(recyclerView, false);
							}
						}

						return false;
					}));
		}

		private void SetTouchDownListener(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX,
			float dY, int actionState, bool isCurrentlyActive)
		{
			recyclerView.SetOnTouchListener(
				new TouchListenerDelegate(
					(view, args) =>
					{
						if (args.Action == MotionEventActions.Down)
						{
							SetTouchUpListener(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
						}
						return false;
					}));
		}

		private void SetTouchUpListener(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX,
			float dY, int actionState, bool isCurrentlyActive)
		{
			recyclerView.SetOnTouchListener(
				new TouchListenerDelegate(
					(recyclerViewInDelegate, outerArgs) =>
					{
						if (outerArgs.Action == MotionEventActions.Up)
						{
							recyclerView.SetOnTouchListener(
								new TouchListenerDelegate(
									(innerView, innerArgs) => { return false; }));

							SetItemsClickable(recyclerView, true);
							_swipeBack = false;

							//							if (_buttonsActions != null && _buttonInstance != null &&
							//								_buttonInstance.Contains(outerArgs.GetX(), outerArgs.GetY()))
							//							{
							//								if (_currentSwipeState == SwipeState.LeftOpen)
							//								{
							//									_buttonsActions.OnLeftClicked(viewHolder.AdapterPosition);
							//								}
							//								else if (_currentSwipeState == SwipeState.RightOpen)
							//								{
							//									_buttonsActions.OnRightClicked(viewHolder.AdapterPosition);
							//								}
							//							}

							_currentSwipeState = SwipeState.Default;


							// folgende codezeile behebt einen darstellungsbug, wenn man eine schaltfläche klickt
							Thread.Sleep(100);
							base.OnChildDraw(c, recyclerView, viewHolder, 0, dY, actionState, isCurrentlyActive);
						}

						return false;
					}));
		}
		

		private void DrawButtons(Canvas canvas, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX,
			float dY, int actionState, bool isCurrentlyActive, SwipeState swipeState, bool swipeEnd)
		{
			var validState = isCurrentlyActive || swipeState != SwipeState.Default;
			

			if (!validState)
				return;

			if (viewHolder is IViewHolderButtonContainer buttonContainer)
			{
				//				int widthSpec = View.MeasureSpec.MakeMeasureSpec(ViewGroup.LayoutParams.WrapContent, MeasureSpecMode.Unspecified);

				int widthSpec = View.MeasureSpec.MakeMeasureSpec(viewHolder.ItemView.Width, MeasureSpecMode.AtMost);
				int heightSpec = View.MeasureSpec.MakeMeasureSpec(viewHolder.ItemView.Height, MeasureSpecMode.AtMost);

				var containerArea = GetContainerArea(dX);
				var container = buttonContainer.CreateButtonContainer(containerArea);
				container.Measure(widthSpec, heightSpec);

				var clippedWidth = GetClippedWidth(viewHolder.ItemView.Width, dX, container.MeasuredWidth);
				if (clippedWidth <= 0)
					return;
				var clippedHeight = GetClippedHeight(viewHolder.ItemView.Height, dY, container.MeasuredHeight);
				if (clippedHeight <= 0)
					return;

				var viewLeft = dX >= 0 ? viewHolder.ItemView.Left : viewHolder.ItemView.Right - clippedWidth;
				var viewTop = viewHolder.ItemView.Top;
				var viewRight = viewLeft + clippedWidth;
				var viewBottom = viewTop + clippedHeight;

				container.Layout(viewLeft, viewTop, viewRight, viewBottom);

				container.SetBackgroundColor(Color.Orange);

				//				var canvasX = dX >= 0 ? viewHolder.ItemView.Left : viewHolder.ItemView.Left + viewHolder.ItemView.Width;

				//					var paint = new Paint();
				//					paint.Color = Color.Aqua;
				//					canvas.DrawRect(viewLeft, viewTop, viewRight, viewBottom, paint);

				// Translate the canvas so the view is drawn at the proper coordinates
				canvas.Save();
				canvas.Translate(viewLeft, viewHolder.ItemView.Top);

				if (swipeEnd)
				{
					viewHolder.ItemView.Invalidate();
					container.Invalidate();
				}

				//Draw the View and clear the translation
				container.Draw(canvas);
				canvas.Restore();
			}
		}

		private static int GetContainerArea(float dX)
		{
			return dX >= 0 ? 0 : 1;
		}

		private int GetClippedHeight(int viewHolderHeight, float dY, int measured)
		{
			return viewHolderHeight;
		}

		private int GetClippedWidth(int viewHolderWidth, float dX, int measured)
		{
			if (viewHolderWidth <= 0)
				return 0;

			return Math.Min(measured, (int) Math.Ceil(Math.Abs(dX)));
		}


		public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
		{
			return MakeMovementFlags(0, ItemTouchHelper.Left | ItemTouchHelper.Right);
		}

		public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder,
			RecyclerView.ViewHolder target)
		{
			return false;
		}

		public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
		{
		}

		private class TouchListenerDelegate : Object, View.IOnTouchListener
		{
			public void Dispose()
			{
				Callback = null;
			}

			public TouchListenerDelegate(Func<View, MotionEvent, bool> callback)
			{
				Callback = callback;
			}

			public Func<View, MotionEvent, bool> Callback { get; set; }

			public bool OnTouch(View v, MotionEvent e)
			{
				return Callback(v, e);
			}
		}
	}

	public interface IViewHolderButtonContainer
	{
		/// <summary>
		/// Erstellt container für die Buttons
		/// </summary>
		/// <param name="containerArea">0 = links, 1 = rechts</param>
		/// <returns></returns>
		View CreateButtonContainer(int containerArea);
	}
}