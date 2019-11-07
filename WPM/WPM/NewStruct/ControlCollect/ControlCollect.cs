using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class ControlCollect : ABaseMode
    {
        public DataTable GoodsCC;
        public String ControlCC;
        public String LabelControlCC;
        public RefItem Item;
        public int IndexTableItem; //индекс строчки
        
        public ControlCollect(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.ControlCollect;
            ControlCC = null;
            Item = new RefItem(SS);
            LabelControlCC = null;
            GoodsCC = new DataTable();
            GoodsCC.Columns.Add("Number", Type.GetType("System.Int32"));
            GoodsCC.Columns.Add("Artikul", Type.GetType("System.String"));
            GoodsCC.Columns.Add("NameItem", Type.GetType("System.String"));
            GoodsCC.Columns.Add("Count", Type.GetType("System.Int32"));
            GoodsCC.Columns.Add("ID", Type.GetType("System.String"));           
        }
        internal override ABaseMode Init()        {

            IndexTableItem = 0;
            return Refresh();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        protected override void ReactionKeyDo(Keys key)
        {
            if ((Helper.IsGreenKey(key) || key == Keys.Enter) && (GoodsCC.Rows.Count > 0))
            {
                Item = new RefItem(SS);
                Item.FoundID(GoodsCC.Rows[IndexTableItem]["ID"].ToString());
                JumpTo(new ItemCard(SS, this));
            }
            else if (key == Keys.Escape)
            {
                if (ControlCC == null)
                {
                    SS.OnReport(new ReportEventArgs("Задача выбрана..."));
                    JumpTo(new ChoiseWork(SS, this));
                }
                else
                {
                    GoodsCC.Clear();
                    ControlCC = null;
                    LabelControlCC = null;
                    Item = new RefItem(SS);
                    Positive("Ожидание команды");
                    return;
                }
            }
            else if (key == Keys.Up)
            {
                if (GoodsCC.Rows.Count == 0)
                {
                    IndexTableItem = 0;
                    return;
                }
                if (IndexTableItem > 0)
                {
                    IndexTableItem--;
                }
                else
                {
                    IndexTableItem = GoodsCC.Rows.Count - 1;
                }
                Positive();
                return;
            }

            else if (key == Keys.Down)
            {
                if (GoodsCC.Rows.Count == 0)
                {
                    IndexTableItem = 0;
                    return;
                }
                if (IndexTableItem < GoodsCC.Rows.Count - 1)
                {
                    IndexTableItem++;
                }
                else
                {
                    IndexTableItem = 0;
                }
                Positive();
                return;
            }

        } // ReactionKeyDo

        protected override void ReactionSCDo(ReactionSCEventArgs e)
        {
            if (e.Ref.GetType() == new RefBox(SS).GetType())
            {
                RefBox Box = e.Ref as RefBox;
                
                Doc doc = new Doc(SS);
                doc.FoundID(Box.GetAttribute("КонтрольНабора").ToString());
                RefreshDoc(doc);
                return;
            }
            else if (e.Ref.GetType() == new RefItem(SS).GetType())
            {
                Item = e.Ref as RefItem;

                string ItemID = Item.ID.ToString();
                string LabelItem = Item.InvCode.ToString().Trim() + " " + Item.Name.ToString().Trim();

                if (ControlCC == null)
                {
                    Negative("Не выбран сборочный лист!");
                    return;
                }
                String TextQuery =
                    "SELECT " +
                        "Goods.$Спр.Товары.ИнвКод as InvCode, " +
                        "DocCC.$КонтрольНабора.Количество as Count " +
                    "FROM DT$КонтрольНабора as DocCC (nolock) " +
                        "JOIN $Спр.Товары as Goods (nolock) " +
                            "ON Goods.Id = DocCC.$КонтрольНабора.Товар " +
                    "WHERE " +
                        "Goods.id = :ItemID " +
                        "and DocCC.iddoc = :iddoc";
                SQL1S.QuerySetParam(ref TextQuery, "ItemID", ItemID);
                SQL1S.QuerySetParam(ref TextQuery, "iddoc", ControlCC);
                DataTable DT;
                if (!SS.ExecuteWithRead(TextQuery, out DT))
                {
                    return;
                }
                if (DT.Rows.Count == 0)
                {
                    Negative("В сборочном нет " + LabelItem);
                    return;
                }
                else
                {
                    JumpTo(new ItemCard(SS, this));

                }
            }
        }
        protected override void ReactionDocDo(Doc doc)
        {
            RefreshDoc(doc);
            return;
        }

        protected override void ReactionSCItemDo(ReactionSCEventArgs e)
        {
            
        }

        private ABaseMode Refresh()
        {
            if (ControlCC == null)
            {
                return Positive();
            }
            GoodsCC.Clear();
            IndexTableItem = 0;
            
            String TextQuery =
                "SELECT " +
                    "DocCC.LineNo_ as Number, " +
                    "Goods.$Спр.Товары.Артикул as Artikul, " +
                    "Goods.Descr as NameItem, " +
                    "Goods.ID as ID, " +
                    "DocCC.$КонтрольНабора.Количество as Count " +
                "FROM DT$КонтрольНабора as DocCC (nolock) " +
                    "JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.Id = DocCC.$КонтрольНабора.Товар " +
                "WHERE " +
                    "DocCC.iddoc = :iddoc";
            SQL1S.QuerySetParam(ref TextQuery, "iddoc", ControlCC);
            if (!SS.ExecuteWithRead(TextQuery, out GoodsCC))
            {
                return Negative();
            }
            if (GoodsCC.Rows.Count == 0)
            {
                return Negative("В сборочном нет товара!");
            }
            return Positive();
        } // Refresh

        private void RefreshDoc(Doc doc)
        {
            if (doc.TypeDoc != "КонтрольНабора")
            {
                Negative("Не верный тип документа!");
                return;
            }
            string TextQuery =
                "SELECT " +
                    "DocCC.$КонтрольНабора.НомерЛиста as Number, " +
                    "Sections.descr as Section, " +
                    "CAST(LEFT(journ.date_time_iddoc,8) as DateTime) as DateTime, " +
                    "journ.DOCNO as DocNumber " +
                "FROM DH$КонтрольНабора as DocCC (nolock) " +
                    "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                        "ON Sections.id = DocCC.$КонтрольНабора.Сектор " +
                    "LEFT JOIN DH$КонтрольРасходной as DocCB (nolock) " +
                        "ON DocCB.iddoc = DocCC.$КонтрольНабора.ДокументОснование " +
                    "LEFT JOIN DH$Счет as Daemond (nolock) " +
                        "ON Daemond.iddoc = DocCB.$КонтрольРасходной.ДокументОснование " +
                    "LEFT JOIN _1sjourn as journ (nolock) " +
                        "ON journ.iddoc = Daemond.iddoc " +
                "WHERE " +
                    "DocCC.iddoc = :iddoc";
            SQL1S.QuerySetParam(ref TextQuery, "iddoc", doc.ID.ToString());
            DataTable DT;
            if (!SS.ExecuteWithRead(TextQuery, out DT))
            {
                return;
            }
            if (DT.Rows.Count == 0)
            {
                Negative("В сборочном нет строк!");
                return;
            }
            ControlCC = doc.ID.ToString();
            LabelControlCC = DT.Rows[0]["DocNumber"].ToString() + " от " + ((DateTime)DT.Rows[0]["DateTime"]).ToString("dd.MM.yy") + " СЕКЦИЯ: " + DT.Rows[0]["Section"].ToString().Trim() + "-" + DT.Rows[0]["Number"].ToString();
            Refresh();
            Positive("ВЫБРАН СБОРОЧНЫЙ ЛИСТ " + LabelControlCC);
            IndexTableItem = 0;         
            

        }

    }
}
