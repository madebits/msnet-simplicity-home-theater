using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Simplicity.SControls
{
    public partial class SList : UserControl
    {
        public SList()
        {
            InitializeComponent();
            this.Disposed += new EventHandler(SList_Disposed);
            this.MouseWheel += new MouseEventHandler(SList_MouseWheel);
            this.panelContent.Dock = DockStyle.None;
            this.panelScroll.Dock = DockStyle.None;
            LoadColors();

            this.PageItemCount = Config.Default.itemsPerPage;
            this.ActivateItemOnClick = Config.Default.activateOnClick;
            this.ActivateOnSpaceKey = true;
            
            Items = null;

            navigation.currentItemChanged += new SListNavigation.DCurrentItemChanged(OnCurrentItemChanged);
            navigation.currentPageChanged += new SListNavigation.DCurrentPageChanged(OnCurrentPageChanged);
            navigation.itemsChanged += new SListNavigation.DItemsChanged(OnItemsChanged);
        }

        private void LoadColors()
        {
            Config.Default.Load();
            this.ForeColor = Config.Default.foreColor;
            this.BackColor = Config.Default.backColor;
            this.panelContent.ForeColor = Config.Default.foreColor;
            this.panelContent.BackColor = Config.Default.backColor;
            this.panelScroll.ForeColor = Config.Default.foreColor;
            this.panelScroll.BackColor = Config.Default.backColor;
            this.Invalidate();
        }

        private void SList_Disposed(object sender, EventArgs e)
        {
            ClearControls();
            if (this.ImageResolver != null)
            {
                this.ImageResolver.Dispose();
                this.ImageResolver = null;
            }
        }

        private SListNavigation navigation = new SListNavigation();

        private void EnableScrollBar()
        {
            bool hasData = HasData;
            this.sbtnHome.Clickable = hasData && !navigation.IsFirstItem;
            this.sbtnEnd.Clickable = hasData && !navigation.IsLastItem;
            this.sbtnUp.Clickable = hasData && !navigation.IsFirstPage;
            this.sbtnDown.Clickable = hasData && !navigation.IsLastPage;
            this.scrollBar.Enabled = hasData;
            this.Focus();
        }

        private void OnItemsChanged(int oldItems, int newItems)
        {
            if (newItems <= 0) newItems = 1;
            this.scrollBar.LargeChange = 1;
            this.scrollBar.Maximum = newItems;
            EnableScrollBar();
        }

        private void OnCurrentItemChanged(int oldItemIndex, int newItemIndex)
        {
            System.Diagnostics.Debug.WriteLine("IDX: " + newItemIndex.ToString());
            if (!withinScrollBar_ValueChanged)
            {
                try
                {
                    withinScrollBar_ValueChanged = true;
                    int v = newItemIndex;
                    if (v < 0) v = 0;
                    if (v > this.scrollBar.Maximum) v = this.scrollBar.Maximum;
                    this.scrollBar.Value = v;
                }
                finally { withinScrollBar_ValueChanged = false; }
            }
            EnableScrollBar();
            SetCurrentItemFocus();
            FireItemSelected();
        }

        private void OnCurrentPageChanged(Point oldPage, Point newPage)
        {
            FillPage(newPage);
        }

        private void FillPage(Point newPage)
        {
            CreateControls();
            bool hasData = HasData;
            try
            {
                this.panelContent.SuspendLayout();

                int topOfPageItemIndex = navigation.Page2Item(new Point(newPage.X, 0));
                if (topOfPageItemIndex < 0) hasData = false;

                for (int i = 0; i < visibleItems.Length; i++)
                {
                    // data
                    bool enabled = hasData;
                    string text = string.Empty;
                    Image img = null;

                    int currentIndex = topOfPageItemIndex + i;
                    if (enabled)
                    {
                        if (currentIndex < navigation.ItemsCount)
                        {
                            text = Items[currentIndex].Text;
                            if (this.ImageResolver != null)
                            {
                                try
                                {
                                    img = this.ImageResolver.GetImage(Items[currentIndex]);
                                }
                                catch { img = null; }
                            }
                        }
                        else
                        {
                            enabled = false;
                        }
                    }
                    visibleItems[i].Text = text;
                    visibleItems[i].Image = img;
                    visibleItems[i].Enabled = enabled;
                }
                //SetCurrentItemFocus();
            }
            finally
            {
                this.panelContent.ResumeLayout();
            }
            this.Invalidate();
            this.Refresh();
        }

        private void SetCurrentItemFocus()
        {
            if (!HasData) return;
            _SetCurrentItemFocus();
        }

        private void _SetCurrentItemFocus()
        {
            if (!HasData) return;
            Point p = navigation.CurrentPage;
            if (!navigation.IsPageValid(p))
            {
                return;
            }
            if (p.Y >= this.visibleItems.Length)
            {
                p.Y = this.visibleItems.Length - 1;
            }
            for (int i = 0; i < visibleItems.Length; i++)
            {
                bool selected = false;
                if ((i == p.Y))
                {
                    if (visibleItems[i].Enabled)
                    {
                        selected = true;
                    }
                }
                if (selected != visibleItems[i].Selected)
                {
                    visibleItems[i].Selected = selected;
                }
            }
        }

        private void SList_Load(object sender, EventArgs e)
        {
            LoadColors();
            InitControls();
        }

        #region data

        public class ItemData
        {
            public ItemData() : this(string.Empty) { }

            public ItemData(string text) : this(text, null) { }

            public ItemData(string text, object tag) { Text = text; Tag = tag; }

            public virtual string Text { get; set; }

            public virtual object Tag { get; set; }

            public static List<ItemData> FromString(List<ItemData> data, string[] d)
            {
                if (d == null) return null;
                if (data == null)
                {
                    data = new List<ItemData>();
                }
                for (int i = 0; i < d.Length; i++)
                {
                    ItemData it = new ItemData(d[i]);
                    data.Add(it);
                }
                return data;
            }
        } //EOIC

        public class ItemImageResolver : IDisposable
        {
            public virtual Image GetImage(ItemData data)
            {
                return null;
            }

            public virtual void Dispose()
            {
            }
        } //EOIC

        public ItemImageResolver ImageResolver { get; set; }

        private List<ItemData> items = null;

        public List<ItemData> Items
        {
            get
            {
                return items;
            }
            private set
            {
                SetItems(value, 0);
            }
        }

        public void SetItems(List<ItemData> items, int startIndex) 
        {
            this.items = items;
            navigation.SetItemCount(((this.items == null) ? 0 : this.items.Count), startIndex);
        }

        public ItemData GetItemByIndex(int index) 
        {
            if (!HasData) return null;
            if (!navigation.IsItemValid(index)) return null;
            return items[index];
        }

        public bool HasData
        {
            get { return navigation.HasData; }
        }

        public int CurrentItemIndex
        {
            get
            {
                return navigation.CurrentItem;
            }
            set
            {
                navigation.CurrentItem = value;
            }
        }

        public void DeleteItem(int itemIndex)
        {
            if (!HasData) return;
            if(!navigation.IsItemValid(itemIndex))
            {
                return;
            }
            items.RemoveAt(itemIndex);
            navigation.Delete(itemIndex);
        }

        private void FireItemSelected()
        {
            if (this.ItemSelected != null)
            {
                this.ItemSelected(this.CurrentItemIndex);
            }
        }

        public int PageItemCount
        {
            get { return navigation.ItemsPerPage; }
            set
            {
                navigation.ItemsPerPage = value;
            }
        }

        #endregion data

        #region navigation

        private void OnMoveFailed()
        {
            SetCurrentItemFocus();
        }

        public bool MoveTop(bool top)
        {
            bool ok = navigation.MoveTop(top);
            if (!ok)
            {
                OnMoveFailed();
            }
            return ok;
        }

        public bool MovePage(bool down)
        {
            bool ok = navigation.MovePage(down);
            if (!ok)
            {
                OnMoveFailed();
            }
            return ok;
        }

        public bool MoveOne(bool next)
        {
            bool ok = navigation.MoveOne(next);
            if (!ok)
            {
                OnMoveFailed();
            }
            return ok;
        }

        private bool MoveToLetter(string prefix)
        {
            bool ok = InnerMoveToLetter(prefix);
            if (!ok)
            {
                OnMoveFailed();
            }
            return ok;
        }

        private bool InnerMoveToLetter(string prefix)
        {
            if (!HasData || string.IsNullOrEmpty(prefix)) return false;
            int start = navigation.CurrentItem;
            for (int i = start + 1; i < navigation.ItemsCount; i++)
            {
                if (Items[i].Text.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    navigation.CurrentItem = i;
                    return true;
                }
            }
            // none found: search from start
            for (int i = 0; i < start; i++)
            {
                if (Items[i].Text.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    navigation.CurrentItem = i;
                    return true;
                }
            }
            return false;
        }

        #endregion navigation

        #region items

        private SListItem[] visibleItems = null;

        private void InitControls()
        {
            CreateControls();
            ResizeControls();
        }

        private void CreateControls()
        {
            try
            {
                if ((visibleItems == null)
                    || (visibleItems.Length != navigation.ItemsPerPage))
                {
                    ClearControls();
                    visibleItems = new SListItem[navigation.ItemsPerPage];
                    for (int i = 0; i < navigation.ItemsPerPage; i++)
                    {
                        SListItem it = new SListItem();
                        it.TabStop = false;
                        it.Selected = false;
                        it.KeyDown += new KeyEventHandler(item_KeyDown);
                        it.GotFocus += new EventHandler(item_GotFocus);
                        it.LostFocus += new EventHandler(item_LostFocus);
                        it.MouseClick += new MouseEventHandler(item_MouseClick);
                        it.MouseDoubleClick += new MouseEventHandler(item_MouseDoubleClick);

                        //it.Text = i.ToString();
                        it.VisibleIndex = i;
                        it.Enabled = false;
                        visibleItems[i] = it;
                    }
                    this.panelContent.Controls.AddRange(visibleItems);
                }
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }
               

        private void ClearControls()
        {
            try
            {
                this.panelContent.Controls.Clear();
                if (visibleItems == null) return;
                for (int i = 0; i < visibleItems.Length; i++)
                {
                    SListItem it = visibleItems[i];
                    visibleItems[i] = null;
                    it.Dispose();
                }
                visibleItems = null;
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private void ResizeControls()
        {
            try
            {
                if (visibleItems == null) return;

                int itemHeight = this.Height / PageItemCount;
                if (itemHeight <= 10)
                {
                    itemHeight = 10;
                }
                Size ss = new Size(itemHeight, this.Height);

                //if (ss.Width < 48) ss.Width = 48;
                //if (ss.Width > 128) ss.Width = 128;

                this.panelContent.Location = new Point(0, 0);
                this.panelContent.Size = new Size(this.Width - ss.Width, this.Height);
                this.panelScroll.Location = new Point(this.Width - ss.Width, 0);
                this.panelScroll.Size = ss;

                ResizeScrollBar(ss);
                int width = this.Width - ss.Width;
                for (int i = 0; i < visibleItems.Length; i++)
                {
                    SListItem it = visibleItems[i];
                    it.Size = new Size(width, itemHeight);
                    it.Location = new Point(0, it.VisibleIndex * itemHeight);
                }
                this.Invalidate();
                this.Refresh();
            }
            catch (Exception ex) { Config.Default.Error(ex); }
        }

        private void ResizeScrollBar(Size ss)
        {
            Size sb = new Size(ss.Width, ss.Width);
            this.sbtnHome.Size = sb;
            this.sbtnUp.Size = sb;
            this.sbtnDown.Size = sb;
            this.sbtnEnd.Size = sb;

            this.sbtnHome.Location = new Point(0, 0);
            this.sbtnUp.Location = new Point(0, sb.Height);
            this.sbtnDown.Location = new Point(0, ss.Height - 2 * sb.Height);
            this.sbtnEnd.Location = new Point(0, ss.Height - sb.Height);

            this.scrollBar.Location = new Point(0, 2 * sb.Height);
            this.scrollBar.Size = new Size(ss.Width, ss.Height - 4 * sb.Height);
        }

        private void item_GotFocus(object sender, EventArgs e)
        {
        }

        private void item_LostFocus(object sender, EventArgs e)
        {
        }

        public event KeyEventHandler ItemKeyDown = null;

        private void item_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = HandleKey(e.KeyCode);
            if (!e.Handled) 
            {
                if (ItemKeyDown != null) 
                {
                    ItemKeyDown(sender, e);
                }
            }
        }

        public bool ActivateItemOnClick
        {
            get;
            set;
        }

        void item_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) { return; }
            ProcessItemActivated(this.CurrentItemIndex, false);
        }

        private void item_MouseClick(object sender, MouseEventArgs e)
        {
            SListItem it = sender as SListItem;
            if (it == null) return;
            if (e.Button == MouseButtons.Left)
            {
                Point p = navigation.CurrentPage;
                p = new Point(p.X, it.VisibleIndex);
                navigation.CurrentPage = p;
                if (ActivateItemOnClick)
                {
                    ProcessItemActivated(this.CurrentItemIndex, false);
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                Point p = navigation.CurrentPage;
                p = new Point(p.X, it.VisibleIndex);
                int itemIndex = navigation.Page2Item(p);
                ProcessItemActivated(itemIndex, true);
            }
        }

        public delegate void ItemActivatedHandler(int itemIndex, bool isContextMenu);
        public event ItemActivatedHandler ItemActivated = null;
        public delegate void ItemSelectedHandler(int itemIndex);
        public event ItemSelectedHandler ItemSelected = null;

        private void ProcessItemActivated(int itemIndex, bool isContextMenu)
        {
            if (ItemActivated != null)
            {
                try
                {
                    ItemActivated(itemIndex, isContextMenu);
                }
                catch (Exception ex) { Config.Default.Error(ex); }
            }
        }

        #endregion items
                

        protected override bool IsInputKey(Keys keyData)
        {
            if (SListItem.IsSInputKey(keyData))
            {
                return true;
            }
            return base.IsInputKey(keyData);
        }

        private void SList_KeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }

        private void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Handled) return;
            if (e.Shift || e.Alt || e.Control) 
            {
                return;
            }
            e.Handled = HandleKey(e.KeyCode);
        }
        
        public bool HandleKey(Keys k)
        {
            bool handled = false;
            switch (k)
            {
                case Keys.Home:
                    MoveTop(true);
                    handled = true;
                    break;
                case Keys.End:
                    MoveTop(false);
                    handled = true;
                    break;
                case Keys.Down:
                    MoveOne(true);
                    handled = true;
                    break;
                case Keys.Up:
                    MoveOne(false);
                    handled = true;
                    break;
                case Keys.PageDown:
                    MovePage(true);
                    handled = true;
                    break;
                case Keys.PageUp:
                    MovePage(false);
                    handled = true;
                    break;
                case Keys.MediaNextTrack:
                    MoveOne(true);
                    handled = true;
                    break;
                case Keys.MediaPreviousTrack:
                    MoveOne(false);
                    handled = true;
                    break;
                case Keys.MediaPlayPause:
                    handled = true;
                    ProcessItemActivated(this.CurrentItemIndex, false);
                    break;
                case Keys.Play:
                    handled = true;
                    ProcessItemActivated(this.CurrentItemIndex, false);
                    break;
                case Keys.SelectMedia:
                    /*
                    if (ActivateOnSpaceKey)
                    {
                        ProcessItemActivated(this.CurrentItemIndex, false);
                        handled = true;
                        break;
                    }
                    */
                    break;
                case Keys.Right:
                    handled = true;
                    ProcessItemActivated(this.CurrentItemIndex, false);
                    break;
                case Keys.Enter:
                    handled = true;
                    ProcessItemActivated(this.CurrentItemIndex, false);
                    break;
                case Keys.Apps:
                    ProcessItemActivated(this.CurrentItemIndex, true);
                    handled = true;
                    break;
                case Keys.Space:
                    if (ActivateOnSpaceKey) 
                    {
                        ProcessItemActivated(this.CurrentItemIndex, false);
                        handled = true;
                    }
                    break;
                default:
                    try
                    {
                        KeysConverter kc = new KeysConverter();
                        string keyChar = kc.ConvertToString(k);
                        handled = MoveToLetter(keyChar);
                    }
                    catch (Exception ex) { Config.Default.Error(ex); }
                    break;
            }
            return handled;
        }

        public bool ActivateOnSpaceKey
        {
            get;
            set;
        }

        private void SList_MouseWheel(object sender, MouseEventArgs e)
        {
            HandleMouseWheel(e);
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
            if (e.Delta == 0) return;
            bool moveDown = e.Delta < 0;
            this.MoveOne(moveDown);
        }

        private void SList_SizeChanged(object sender, EventArgs e)
        {
            ResizeControls();
        }

        #region scroll

        private bool withinScrollBar_ValueChanged = false;

        private void scrollBar_ValueChanged(object sender, EventArgs e)
        {
            if (withinScrollBar_ValueChanged) return;
            try
            {
                withinScrollBar_ValueChanged = true;
                navigation.CurrentItem = scrollBar.Value;
            }
            finally
            {
                withinScrollBar_ValueChanged = false;
            }
        }

        private void sbtnHome_Click(object sender, EventArgs e)
        {
            this.MoveTop(true);
        }

        private void sbtnUp_Click(object sender, EventArgs e)
        {
            this.MovePage(false);
        }

        private void sbtnDown_Click(object sender, EventArgs e)
        {
            this.MovePage(true);
        }

        private void sbtnEnd_Click(object sender, EventArgs e)
        {
            this.MoveTop(false);
        }

        #endregion scroll
    }
}