using MongoDB.Driver;
using Xunit;

namespace Genocs.QueryBuilder.UnitTests.DynamicQuery;

//public class SqlServerDynamicQueriesUnitTests
//{
//    private async Task<List<UnwindPromotionWallet>> GetUnwindItems()
//    {
//        IMongoCollection<PromotionWallet> promotionWallets = TestHelper.GetPromotionWalletCollection();

//        var aggregates = await promotionWallets
//                                        .Aggregate()
//                                      .Unwind(x => x.CreditCards, new AggregateUnwindOptions<UnwindPromotionWallet_1> { IncludeArrayIndex = "CreditCardsCount", PreserveNullAndEmptyArrays = true })
//                                      .Unwind(x => x.FidelityCards, new AggregateUnwindOptions<UnwindPromotionWallet_2> { IncludeArrayIndex = "FidelityCardsCount", PreserveNullAndEmptyArrays = true })
//                                      .Unwind(x => x.PartnerAccounts, new AggregateUnwindOptions<UnwindPromotionWallet_3> { IncludeArrayIndex = "PartnerAccountsCount", PreserveNullAndEmptyArrays = true })
//                                      .Unwind(x => x.Transactions, new AggregateUnwindOptions<UnwindPromotionWallet> { IncludeArrayIndex = "TransactionsCount", PreserveNullAndEmptyArrays = true })
//                                      .Unwind(x => x["Bs.Cs"])
//                                      .Group(new BsonDocument { { "_id", "$Bs.Cs.Name" }, { "total", new BsonDocument("$sum", "$Bs.Cs.Amount") } })
//                                      .Sort(new BsonDocument("total", -1))
//                                      .ToListAsync();

//        List<UnwindPromotionWallet> result = new List<UnwindPromotionWallet>();

//        foreach (var aggregate in aggregates)
//        {
//            var tmp = new UnwindPromotionWallet(aggregate.MemberId,
//                                                aggregate.MobileNumber,
//                                                aggregate.MobilePrefix,
//                                                aggregate.Email,
//                                                aggregate.MobileLanguage,
//                                                aggregate.CountryOfResidence,
//                                                aggregate.Currency,
//                                                aggregate.SignIn,
//                                                //aggregate.Membership,
//                                                aggregate.CreatedAt,
//                                                aggregate.UpdatedAt);

//            tmp.CreditCards = aggregate.CreditCards.FirstOrDefault();

//            result.Add(tmp);
//        }

//        return result;
//    }

//    [Fact]
//    public async Task MultipleUnwindTestAsync()
//    {
//        await Task.CompletedTask;
//        //var result = await GetUnwindItems();
//    }


//    [Fact]
//    public async Task ApplyAndOrOperatorToAStringTest()
//    {
//        await Task.CompletedTask;

//        var unwindedItems = await GetUnwindItems();

//        QueryItem queryItem = new QueryItem(propertyName: "MemberId", propertyValue: "UK9I1LONH or (HRNX5XOFA or MRNXFXOFA)");

//        IQueryable<UnwindPromotionWallet> unwindedItemsQuery = unwindedItems.AsQueryable();
//        unwindedItemsQuery = unwindedItemsQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<UnwindPromotionWallet>(queryItem, "UnwindPromotionWallet"));

//        //var result = unwindedItemsQuery.ToList();
//        //Assert.NotEmpty(result);
//    }

    //[Fact]
    //public async Task ApplyEqualOperatorToNullableIntReturnItemsTest()
    //{
    //    await Task.CompletedTask;

    //    //var unwindedItems = await GetUnwindItems();

    //    //QueryItem queryItem = new QueryItem(propertyName: "PartnerAccountsCount", propertyValue: "1", propertyType: "int");

    //    //IQueryable<UnwindPromotionWallet> unwindedItemsQuery = unwindedItems.AsQueryable();
    //    //unwindedItemsQuery = unwindedItemsQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<UnwindPromotionWallet>(queryItem, "UnwindPromotionWallet"));

    //    //var result = unwindedItemsQuery.ToList();
    //    //Assert.NotEmpty(result);
    //}


    //[Fact]
    //public async Task ApplyEqualOperatorToNullableIntReturnEmptyTest()
    //{
    //    await Task.CompletedTask;

    //    //var unwindedItems = await GetUnwindItems();

    //    //QueryItem queryItem = new QueryItem(propertyName: "PartnerAccountsCount", propertyValue: "4", propertyType: "int");

    //    //IQueryable<UnwindPromotionWallet> unwindedItemsQuery = unwindedItems.AsQueryable();
    //    //unwindedItemsQuery = unwindedItemsQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<UnwindPromotionWallet>(queryItem, "UnwindPromotionWallet"));

    //    //var result = unwindedItemsQuery.ToList();
    //    //Assert.Empty(result);
    //}



    //[Fact]
    //public async Task ApplyGraterThanOperatorToNullableIntTest()
    //{
    //    await Task.CompletedTask;

    //    //var unwindedItems = await GetUnwindItems();

    //    //QueryItem queryItem = new QueryItem(propertyName: "PartnerAccountsCount", propertyValue: "4", propertyType: "int", operatorType: "gt");

    //    //IQueryable<UnwindPromotionWallet> unwindedItemsQuery = unwindedItems.AsQueryable();
    //    //unwindedItemsQuery = unwindedItemsQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<UnwindPromotionWallet>(queryItem, "UnwindPromotionWallet"));

    //    //var result = unwindedItemsQuery.ToList();
    //    //Assert.Empty(result);
    //}


    //[Fact]
    //public async Task OperatorWithQueryItemTest()
    //{
    //    await Task.CompletedTask;

    //    //var unwindedItems = await GetUnwindItems();

    //    //QueryItem queryItem = new QueryItem(propertyName: "MemberId", propertyValue: "UK9I1LONH or (HRNX5XOFA or MRNXFXOFA)");

    //    //IQueryable<UnwindPromotionWallet> unwindedItemsQuery = unwindedItems.AsQueryable();
    //    //unwindedItemsQuery = unwindedItemsQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<UnwindPromotionWallet>(queryItem, "UnwindPromotionWallet"));

    //    //var result = unwindedItemsQuery.ToList();
    //    //Assert.NotEmpty(result);
    //}


    //[Fact]
    //public async Task OperatorWithQueryItemListTest()
    //{
    //    await Task.CompletedTask;

    //    //var unwindedItems = await GetUnwindItems();

    //    //QueryItem queryItem1 = new QueryItem(propertyName: "MemberId", propertyValue: "UK9I1LONH or (HRNX5XOFA or MRNXFXOFA)");
    //    //QueryItem queryItem2 = new QueryItem(propertyName: "MobilePrefix", propertyValue: "+91");


    //    //var queryItemList = new List<QueryItem> { queryItem1, queryItem2 };

    //    //IQueryable<UnwindPromotionWallet> unwindedItemsQuery = unwindedItems.AsQueryable();
    //    //unwindedItemsQuery = unwindedItemsQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<UnwindPromotionWallet>(queryItemList, "UnwindPromotionWallet"));

    //    //var result = unwindedItemsQuery.ToList();
    //    //Assert.NotEmpty(result);
    //}
//}