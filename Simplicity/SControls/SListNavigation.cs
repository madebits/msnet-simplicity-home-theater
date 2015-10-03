using System.Drawing;

namespace Simplicity.SControls
{
    class SListNavigation
    {
        public delegate void DCurrentItemChanged(int oldItemIndex, int newItemIndex);
        public delegate void DCurrentPageChanged(Point oldPage, Point newPage);
        public delegate void DItemsChanged(int oldCount, int newCount);
        public DCurrentItemChanged currentItemChanged = null;
        public DCurrentPageChanged currentPageChanged = null;
        public DItemsChanged itemsChanged = null;

        private int itemCount = 0;
        private int itemsPerPage = 5;
        private int currentItem = -1;

        public int ItemsCount
        {
            get { return itemCount; }
            set
            {
                SetItemCount(value, 0);
            }
        }

        private bool forceItemIndexUpdate = false;
        public void SetItemCount(int count, int startItem) 
        {
            if (count < 0) count = 0;
            int oldCount = itemCount;
            itemCount = count;
            if (itemsChanged != null)
            {
                itemsChanged(oldCount, itemCount);
            }
            if (HasData)
            {
                if (startItem < 0) startItem = 0;
                if (startItem >= count) startItem = count - 1;
            }
            else 
            {
                startItem = -1;
            }
            forceItemIndexUpdate = true;
            CurrentItem = startItem;
        }
        
        public bool HasData
        {
            get { return (ItemsCount > 0); }
        }

        public void Delete(int itemIndex)
        {
            if (!HasData) return;
            if (!IsItemValid(itemIndex)) return;
            int nextItemIndex = itemIndex;
            itemCount = itemCount - 1;
            if (nextItemIndex >= itemCount) nextItemIndex = itemCount - 1;
            SetItemCount(itemCount, nextItemIndex);
        }

        public int ItemsPerPage
        {
            get { return itemsPerPage; }
            set
            {
                if (value <= 0) return;
                // reset
                int itemIndex = CurrentItem;
                itemsPerPage = value;
                CurrentItem = itemIndex;
            }
        }

        public int PageCount
        {
            get
            {
                int p = ItemsCount / ItemsPerPage;
                int r = ItemsCount % ItemsPerPage;
                if (r > 0) p++;
                return p;
            }
        }

        public bool IsPageValid(Point p)
        {
            int itemIndex = Page2Item(p);
            return IsItemValid(itemIndex);
        }

        public Point Item2Page(int itemIndex)
        {
            Point result = new Point(-1, -1);
            if ((itemIndex >= 0) && (itemIndex < ItemsCount))
            {
                result.X = itemIndex / ItemsPerPage;
                result.Y = itemIndex % ItemsPerPage;
            }
            return result;
        }

        public int Page2Item(Point p)
        {
            if ((p.X < 0) || (p.Y < 0)) return -1;
            return p.X * ItemsPerPage + p.Y;
        }

        public int CurrentItem
        {
            get
            {
                if (!HasData) return -1;
                return currentItem;
            }
            set
            {
                bool force = forceItemIndexUpdate;
                forceItemIndexUpdate = false;

                if (value >= this.ItemsCount) value = this.ItemsCount - 1;
                int oldValue = currentItem;
                currentItem = value;
                if (force || (oldValue != currentItem))
                {
                    if (currentPageChanged != null)
                    {
                        Point oldPage = Item2Page(oldValue);
                        Point newPage = Item2Page(currentItem);
                        if (force || (oldPage.X != newPage.X))
                        {
                            currentPageChanged(oldPage, newPage);
                        }
                    }
                    if (currentItemChanged != null)
                    {
                        currentItemChanged(oldValue, currentItem);
                    }
                }
            }
        }

        public bool IsItemValid(int itemIndex)
        {
            if (itemIndex < 0) return false;
            if (itemIndex >= this.ItemsCount) return false;
            return true;
        }

        public Point CurrentPage
        {
            get { return Item2Page(CurrentItem); }
            set
            {
                CurrentItem = Page2Item(value);
            }
        }

        public bool MovePage(bool down)
        {
            if (!HasData) return false;
            Point newPage = CurrentPage;
            if (down)
            {
                newPage.X++;
            }
            else
            {
                newPage.X--;
            }
            newPage.Y = 0;
            if (!IsPageValid(newPage))
            {
                return false;
            }
            CurrentPage = newPage;
            return true;
        }

        public bool MoveTop(bool top)
        {
            if (!HasData) return false;
            int newItemIdx = top ? 0 : this.ItemsCount - 1;
            if (CurrentItem == newItemIdx)
            {
                return false;
            }
            CurrentItem = newItemIdx;
            return true;
        }

        public bool MoveOne(bool next)
        {
            if (!HasData) return false;
            int newItemIdx = CurrentItem;
            if (next)
            {
                newItemIdx++;
            }
            else
            {
                newItemIdx--;
            }
            if (!IsItemValid(newItemIdx))
            {
                return false;
            }
            CurrentItem = newItemIdx;
            return true;
        }

        public bool IsFirstItem
        {
            get
            {
                if (!HasData) return false;
                return (CurrentItem == 0);
            }
        }

        public bool IsLastItem
        {
            get
            {
                if (!HasData) return false;
                return (CurrentItem == (ItemsCount - 1));
            }
        }

        public bool IsFirstPage
        {
            get
            {
                if (!HasData) return false;
                return (CurrentPage.X == 0);
            }
        }

        public bool IsLastPage
        {
            get
            {
                if (!HasData) return false;
                return (CurrentPage.X == (PageCount - 1));
            }
        }
    }//EOC
}