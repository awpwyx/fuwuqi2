using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
namespace SF.K3Cloud.Report.Plugin
{
   //简单账表2服务器插件
    [Description("测试报表")]
    public class SFTest : SysReportBaseService
    {
        public override void Initialize()
        {
            //base.ReportProperty.DetailReportId=("2a0bf3eec54f4d4289831447ca01d5f1");
            base.Initialize();                      
            // 简单账表类型：普通、树形、分页            
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            // 报表名称            
            this.ReportProperty.ReportName = new LocaleValue("测试报表", base.Context.UserLocale.LCID);
            //             
            this.IsCreateTempTableByPlugin = true;            // 不利用动态构建列            
            this.ReportProperty.IsUIDesignerColumns = false;
            //             
            this.ReportProperty.IsGroupSummary = true;
            //             
            this.ReportProperty.SimpleAllCols = false;
            // 单据主键：两行FID相同，则为同一单的两条分录，单据编号可以不重复显示            
            this.ReportProperty.PrimaryKeyFieldName = "line";
            //             
            this.ReportProperty.IsDefaultOnlyDspSumAndDetailData = true;
            // 报表主键字段名：默认为FIDENTITYID，可以修改            
            this.ReportProperty.IdentityFieldName = "line";        
        }
        public override string GetTableName()
        {
            var result = base.GetTableName();
            return result;
        }
        /// <summary>        
        /// 向报表临时表，插入报表数据        
        /// </summary>        
        /// <param name="filter"></param>        
        /// <param name="tableName"></param>        
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            Dictionary<string, object> fil = filter.CustomParams;
            Dictionary<string, object> fil1 = fil["OpenParameter"] as Dictionary<string, object>;
            string project = fil1["F_DEV_PRONO"].ToString();
            string begintime = fil1["F_DEV_BEGINTIME"].ToString();
            string endtime = fil1["F_DEV_ENDTIME"].ToString();


            //Dictionary<string, object> fil1 = filter.ite;
            //new System.Collections.Generic.Mscorlib_DictionaryDebugView<string, object>((new System.Collections.Generic.Mscorlib_DictionaryDebugView<string, object>(((Kingdee.BOS.Core.Report.RptParams)filter).CustomParams).Items[0]).Value).Items["F_DEV_PRONO"]
            //var paramValue = filter.CustomParams.Values.Items;
            base.BuilderReportSqlAndTempTable(filter, tableName);
            SortedList<int, string> list = new SortedList<int, string>();//存储过滤条件            
            DynamicObject customFil = filter.FilterParameter.CustomFilter;//获取快捷页面的参数                                                                     
            // 默认排序字段：需要从filter中取用户设置的排序字段            
            string seqFld = string.Format(base.KSQL_SEQ, "line");
            string sql = string.Format(@"/*dialect*/ select top 100 percent row_number() over (order by department,project,row,cast(ord as int) ) as 'line',department,project,row,source,budget,ord,convert(decimal(18,2),price/10000) as 'price',convert(decimal(18,2),fdebitfor/10000) as 'fdebitfor',prcent,convert(decimal(18,2),balance/10000) as 'balance' into {1} from (
select  '1' as 'row', T_BD_DEPARTMENT_L.FNAME as 'department',e.FDATAVALUE as 'project',f.FDATAVALUE as 'source',T_BAS_ASSISTANTDATAENTRY_L.FDATAVALUE as 'budget',T_BAS_ASSISTANTDATAENTRY_L.FDESCRIPTION as 'ord',Budget.price, 
convert(decimal(20,2),  ISNULL( cost.fdebitfor/2,0))as 'fdebitfor',cast(convert(decimal(20,2), case when price=0 then 0 else (ISNULL( cost.fdebitfor/2,0)/price )end)*100 as varchar	)+'%' as 'prcent',convert(decimal(20,2),price-ISNULL( cost.fdebitfor/2,0)) as 'balance'

from (select b.FENTRYID as 'budgeid',T_BAS_ASSISTANTDATAENTRY.FENTRYID as 'ProjectID',a.FENTRYID as 'sourceID',T_BD_DEPARTMENT.FDEPTID as 'deptid',SUM( fprice)as price 
from T_JX_Budget left join 
T_JX_BudgetEntry on T_JX_Budget.ID=T_JX_BudgetEntry.FInterId
left join T_BAS_ASSISTANTDATAENTRY on T_JX_Budget.FProjectNumber=T_BAS_ASSISTANTDATAENTRY.FNUMBER and T_BAS_ASSISTANTDATAENTRY.FID = '573449f4900500'
left join T_BAS_ASSISTANTDATAENTRY a on T_JX_Budget.FSourceFunds=a.FNUMBER and a.FID = '573415629004f4'
left join T_BD_DEPARTMENT on T_BD_DEPARTMENT.FNUMBER=T_JX_Budget.FDepartment
left join T_BAS_ASSISTANTDATAENTRY b
on T_JX_BudgetEntry.FBudgetNumber=b.FNUMBER
and b.FID='5efb3ed66c097a'

group by b.FENTRYID,T_BAS_ASSISTANTDATAENTRY.FENTRYID ,a.FENTRYID,T_BD_DEPARTMENT.FDEPTID )Budget
left join (

select SUM(T_GL_VoucherEntry.FDEBIT)as 'fdebitfor',t_bd_flexitemdetailv.FFLEX5 as 'departmentid',t_bd_flexitemdetailv.FF100002 as 'sourseid',t_bd_flexitemdetailv.FF100003 as 'projectid',PAEZ_t_Cust100004.F_PAEZ_ASSISTANT  as 'budgeid'
from T_GL_BALANCE_100200
left join T_GL_Voucher  
on T_GL_VoucherEntry.FDetailID=t_bd_flexitemdetailv.FID
left join t_bd_flexitemdetailv
on T_GL_BALANCE_100200.FDetailID=t_bd_flexitemdetailv.FID
left join PAEZ_t_Cust100004
on PAEZ_t_Cust100004.F_PAEZ_BASE=t_bd_flexitemdetailv.FFLEX9
where FAccountID=100805 and T_GL_Voucher.FDATE>'{3}' and T_GL_Voucher.FDATE<'{4}'
group by t_bd_flexitemdetailv.FFLEX5,t_bd_flexitemdetailv.FF100002,t_bd_flexitemdetailv.FF100003,PAEZ_t_Cust100004.F_PAEZ_ASSISTANT )cost
on  Budget.ProjectID=cost.projectid
and Budget.sourceID=cost.sourseid
and Budget.deptid=cost.departmentid
and Budget.budgeid=cost.budgeid
left join T_BAS_ASSISTANTDATAENTRY_L
on T_BAS_ASSISTANTDATAENTRY_L.FENTRYID=Budget.budgeid
left join T_BAS_ASSISTANTDATAENTRY_L e
on e.FENTRYID=Budget.ProjectID
left join T_BAS_ASSISTANTDATAENTRY e_n
on e_n.FENTRYID=e.FENTRYID
left join T_BAS_ASSISTANTDATAENTRY_L f
on f.FENTRYID=Budget.sourceID
left join T_BD_DEPARTMENT_L
on T_BD_DEPARTMENT_L.FDEPTID=Budget.deptid
where  e_n.FNUMBER = {2}
union
select  '2' as 'row', T_BD_DEPARTMENT_L.FNAME as 'department',e.FDATAVALUE+'(小计)' as 'project','' as 'source',''as 'budget','99' as 'ord'
,sum(Budget.price) as 'price',
sum(convert(decimal(20,2),  ISNULL( cost.fdebitfor/2,0)))as 'fdebitfor'
,cast(convert(decimal(20,2), case when sum(Budget.price)=0 then 0 else sum(ISNULL( cost.fdebitfor/2,0))/sum(Budget.price )end)*100 as varchar	)+'%' as 'prcent'
,sum(convert(decimal(20,2),price-ISNULL( cost.fdebitfor/2,0))) as 'balance'

  from (select b.FENTRYID as 'budgeid',T_BAS_ASSISTANTDATAENTRY.FENTRYID as 'ProjectID',a.FENTRYID as 'sourceID',T_BD_DEPARTMENT.FDEPTID as 'deptid',SUM( fprice)as price 
from T_JX_Budget left join 
T_JX_BudgetEntry on T_JX_Budget.ID=T_JX_BudgetEntry.FInterId
left join T_BAS_ASSISTANTDATAENTRY on T_JX_Budget.FProjectNumber=T_BAS_ASSISTANTDATAENTRY.FNUMBER and T_BAS_ASSISTANTDATAENTRY.FID = '573449f4900500'
left join T_BAS_ASSISTANTDATAENTRY a on T_JX_Budget.FSourceFunds=a.FNUMBER and a.FID = '573415629004f4'
left join T_BD_DEPARTMENT on T_BD_DEPARTMENT.FNUMBER=T_JX_Budget.FDepartment
left join T_BAS_ASSISTANTDATAENTRY b
on T_JX_BudgetEntry.FBudgetNumber=b.FNUMBER
and b.FID='5efb3ed66c097a'

group by b.FENTRYID,T_BAS_ASSISTANTDATAENTRY.FENTRYID ,a.FENTRYID,T_BD_DEPARTMENT.FDEPTID )Budget
left join (

select SUM(T_GL_VoucherEntry.FDEBIT)as 'fdebitfor',t_bd_flexitemdetailv.FFLEX5 as 'departmentid',t_bd_flexitemdetailv.FF100002 as 'sourseid',t_bd_flexitemdetailv.FF100003 as 'projectid',PAEZ_t_Cust100004.F_PAEZ_ASSISTANT  as 'budgeid'
from T_GL_BALANCE_100200
on T_GL_VoucherEntry.FDetailID=t_bd_flexitemdetailv.FID
where FAccountID=100805 and T_GL_Voucher.FDATE>'{3}' and T_GL_Voucher.FDATE<'{4}'
left join t_bd_flexitemdetailv
on T_GL_BALANCE_100200.FDetailID=t_bd_flexitemdetailv.FID
left join PAEZ_t_Cust100004
on PAEZ_t_Cust100004.F_PAEZ_BASE=t_bd_flexitemdetailv.FFLEX9
where FAccountID=100805 
group by t_bd_flexitemdetailv.FFLEX5,t_bd_flexitemdetailv.FF100002,t_bd_flexitemdetailv.FF100003,PAEZ_t_Cust100004.F_PAEZ_ASSISTANT )cost
on  Budget.ProjectID=cost.projectid
and Budget.sourceID=cost.sourseid
and Budget.deptid=cost.departmentid
and Budget.budgeid=cost.budgeid
left join T_BAS_ASSISTANTDATAENTRY_L
on T_BAS_ASSISTANTDATAENTRY_L.FENTRYID=Budget.budgeid
left join T_BAS_ASSISTANTDATAENTRY_L e
on e.FENTRYID=Budget.ProjectID
left join T_BAS_ASSISTANTDATAENTRY e_n
on e_n.FENTRYID=e.FENTRYID
left join T_BAS_ASSISTANTDATAENTRY_L f
on f.FENTRYID=Budget.sourceID
left join T_BD_DEPARTMENT_L
on T_BD_DEPARTMENT_L.FDEPTID=Budget.deptid

where (Budget.price !=0 or cost.fdebitfor !=0)

and e_n.FNUMBER ={2}
group by T_BD_DEPARTMENT_L.FNAME ,e.FDATAVALUE

)a 
where department is not null
order by department,project,row,cast(ord as int) 
",
seqFld, tableName, project,begintime,endtime);
            DBUtils.ExecuteDynamicObject(this.Context, sql.ToString());
        }
        /// <summary>        
        /// 构建报表列        
        /// </summary>        
        /// <param name="filter"></param>        
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();
            // 订单编号            
            var line = header.AddChild("line", new LocaleValue("序号"));
            line.Mergeable = true;
            line.Index = 0;
            // 客户编码            
            var department = header.AddChild("department", new LocaleValue("部门"));
            department.Mergeable = true;
            department.Index = 1;
            // 客户名称            
            var FDetailID = header.AddChild("project", new LocaleValue("项目"));
            FDetailID.Mergeable = true;
            FDetailID.Index = 2;        
            // 客户名称            
            var budget = header.AddChild("budget", new LocaleValue("预算项目"));
            budget.Mergeable = true;
            budget.Index = 3;          
            // 客户名称            
            var price = header.AddChild("price", new LocaleValue("预算金额（单位：万元）"));
            price.Mergeable = true;
            price.Index = 4;
            // 客户名称            
            var fdebitfor = header.AddChild("fdebitfor", new LocaleValue("花费金额（单位：万元）"));
            fdebitfor.Mergeable = true;
            fdebitfor.Index = 5;
            // 客户名称            
            var prcent = header.AddChild("prcent", new LocaleValue("花费百分比"));
            prcent.Mergeable = true;
            prcent.Index = 6;
            // 客户名称            
            var balance = header.AddChild("balance", new LocaleValue("余额（单位：万元）"));
            balance.Mergeable = true;
            balance.Index = 7;
            return header;
        }


        protected override string GetSummaryColumsSQL(List<SummaryField> summaryFields)
        {
            var result = base.GetSummaryColumsSQL(summaryFields);
            return result;
        }
        protected override System.Data.DataTable GetListData(string sSQL)
        {
            var result = base.GetListData(sSQL);
            return result;
        }
        protected override System.Data.DataTable GetReportData(IRptParams filter)
        {
            var result = base.GetReportData(filter);
            return result;
        }
        protected override System.Data.DataTable GetReportData(string tablename, IRptParams filter)
        {
            var result = base.GetReportData(tablename, filter);
            return result;
        }
        public override int GetRowsCount(IRptParams filter)
        {
            var result = base.GetRowsCount(filter);
            return result;
        }
        public override void CloseReport()
        {
            base.CloseReport();
        }
        protected override void CreateTempTable(string sSQL)
        {
            base.CreateTempTable(sSQL);
        }
        public override void DropTempTable()
        {
            base.DropTempTable();
        }
    }
}

