using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using App1.Domain;

namespace App1
{
	[Activity(Label = "App1", MainLauncher = true)]
	public class MainActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			var mainView = LayoutInflater.Inflate(Resource.Layout.Main, null, true) as LinearLayout;
			SetContentView(mainView);
			mainView.SetBackgroundColor(Color.Red);
			var recyclerView = new RecyclerView(this);
			var recyclerAdapter = new RecyclerAdapter();
			for (int i = 0; i < 30; i++)
			{
				recyclerAdapter.Items.Add($"Entry {i}.");
			}
			recyclerView.SetAdapter(recyclerAdapter);
			var linearLayoutManager = new LinearLayoutManager(this);
			var swipeController = new LeftRightSwipeController();
			var ith = new ItemTouchHelper(swipeController);
			ith.AttachToRecyclerView(recyclerView);
			recyclerView.SetLayoutManager(linearLayoutManager);
			recyclerView.SetBackgroundColor(Color.Orange);
			mainView.AddView(recyclerView);
		}

		public class RecyclerAdapter : RecyclerView.Adapter
		{
			public List<string> Items { get; set; } = new List<string>();

			public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
			{
				var data = Items[position];
				var textView = holder.ItemView.FindViewById<TextView>(Android.Resource.Id.Text1);
				if (textView != null)
				{
					textView.Text = data;
				}
			}

			public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
			{
				var inflater = Android.Views.LayoutInflater.FromContext(parent.Context);
				var view = inflater.Inflate(Android.Resource.Layout.SimpleListItem1, parent, false);
				return new CustomViewHolder(view);
			}

			public override int ItemCount => Items.Count;
		}

		public class CustomViewHolder : RecyclerView.ViewHolder, IViewHolderButtonContainer
		{
			public CustomViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
			{
			}

			public CustomViewHolder(View itemView) : base(itemView)
			{
			}

			private View _leftContainer;
			private View _rightContainer;

			public View CreateButtonContainer(int containerArea)
			{
				if (containerArea == 0)
				{
					return _leftContainer ?? (_leftContainer = CreateContainer(containerArea));
				}
				else
				{
					return _rightContainer ?? (_rightContainer = CreateContainer(containerArea));
				}
			}

			private string GetButtonText(int containerArea)
			{
				return $"Button area {containerArea}";
			}

			private View CreateContainer(int containerArea)
			{
				var textView = new TextView(Android.App.Application.Context);
				textView.SetBackgroundColor(Color.Blue);
				textView.Text = GetButtonText(containerArea);
				return textView;
			}
		}
	}
}

