using Genocs.QueryBuilder.UnitTests.Models;
using MongoDB.Driver;
using Xunit;

namespace Genocs.QueryBuilder.UnitTests.DynamicQuery;

/******************************************/
/*** String                               */
/*** int                                  */
/*** DateTime                             */
/*** decimal                              */
/*** bool                                 */
/*** nullable int                         */
/*** nullable DateTime                    */
/*** nullable decimal                     */
/*** nullable bool                        */
/******************************************/

public class InMemoryDynamicQueriesUnitTests
{
    private async Task<List<User>> GetUsers()
    {
        List<User> users = new List<User>
        {
            new User { Id = 1, FirstName = "Giovanni", LastName = "Nocco", Age = 53, IsActive = true },
            new User { Id = 2, FirstName = "Giulio", LastName = "Nocco", Age = 19, IsActive = true },
            new User { Id = 3, FirstName = "Vittoria", LastName = "Nocco", Age = 17, IsActive = false },
            new User { Id = 4, FirstName = "Emanuele", LastName = "Nocco", Age = 54, IsActive = true, DateOfBirth = DateTime.Parse("1968-10-24"), Childs = 4 }
        };

        Address address = new Address { City = "Milano" };

        users.Add(new User { Id = 5, FirstName = "Joshua", LastName = "Nocco", Age = 61, IsActive = true, DateOfBirth = DateTime.Parse("1965-10-24"), Childs = 2, Address = address });

        return await Task.Run(() => users);
    }

    private async Task<List<Order>> GetOrders()
    {
        List<Order> orders = new List<Order>
        {
            new Order(1, 1),
            new Order(2, 2),
            new Order(3, 1),
            new Order(4, 2)
        };

        Address address = new Address { City = "Milano" };

        // users.Add(new User { Id = 5, FirstName = "Joshua", LastName = "Nocco", Age = 61, IsActive = true, DateOfBirth = DateTime.Parse("1965-10-24"), Childs = 2, Address = address });

        return await Task.Run(() => orders);
    }

    [Fact]
    public async Task MultipleUnwindTestAsync()
    {
        var result = await GetUsers();
    }

    [Fact]
    public async Task ApplyAndOrOperatorToAStringTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "FirstName", propertyValue: "Giovanni or (Emanuele and Giulio)");

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.Empty(result);
    }

    [Fact]
    public async Task ApplyIntegerAge53OperatorToIntReturnItemsTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "Age", propertyValue: "53", propertyType: "int");

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ApplyEqual22OperatorToIntReturnItemsTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "Age", propertyValue: "19", propertyType: "int");

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ApplyEqual11OperatorToIntReturnEmptyItemsTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "Age", propertyValue: "11", propertyType: "int");

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.Empty(result);
    }


    [Fact]
    public async Task ApplyBoolCamelCaseOperatorReturnItemsTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "IsActive", propertyValue: "true", propertyType: "bool");

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ApplyBoolLowecaseOperatorReturnItemsTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "IsActive", propertyValue: "true", propertyType: "bool");

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.NotEmpty(result);
        Assert.True(result.First().IsActive);
    }

    [Fact]
    public async Task ApplyBoolLowecaseFalseOperatorReturnItemsTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "IsActive", propertyValue: "false", propertyType: "bool");

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.NotEmpty(result);
        Assert.False(result.First().IsActive);
    }

    [Fact]
    public async Task ApplyDateTimeOperatorReturnItemsTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "DateOfBirth", propertyValue: "1968-10-24", propertyType: "date");

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.NotEmpty(result);
        Assert.Equal(4, result.First().Id);
    }

    [Fact]
    public async Task ApplyGreaterThanDateTimeOperatorReturnItemsTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "DateOfBirth", propertyValue: "1968-10-23", propertyType: "date", operatorType: QueryOperator.GreaterThan);

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.NotEmpty(result);
        Assert.Equal(4, result.First().Id);
    }

    [Fact]
    public async Task ApplyLessThanDateTimeOperatorReturnItemsTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "DateOfBirth", propertyValue: "1968-10-23", propertyType: "date", operatorType: QueryOperator.LessThan);

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task ApplyGreaterThanOperatorToIntReturnItemsTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "Age", propertyValue: "53", propertyType: "int", operatorType: QueryOperator.GreaterThan);

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ApplyGreaterThanOperatorToIntGetItemsTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "Age", propertyValue: "2", propertyType: "int", operatorType: QueryOperator.GreaterThan);

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.NotEmpty(result);
    }


    [Fact]
    public async Task ApplyGreaterThanOperatorToNullableIntGetItemTest()
    {
        var users = await GetUsers();

        QueryItem queryItem = new QueryItem(propertyName: "Childs", propertyValue: "2", propertyType: "numeric", operatorType: QueryOperator.GreaterThan);

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItem, "User"));

        var result = usersQuery.ToList();
        Assert.Single(result);
    }

    [Fact]
    public async Task ApplyOperatorWithQueryItemListTest()
    {
        var users = await GetUsers();

        QueryItem queryItem1 = new QueryItem(propertyName: "FirstName", propertyValue: "Giulio");
        QueryItem queryItem2 = new QueryItem(propertyName: "Age", propertyValue: "19", propertyType: "int");

        var queryItemList = new List<QueryItem> { queryItem1, queryItem2 };

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItemList, "User"));

        var result = usersQuery.ToList();
        Assert.Single(result);
    }

    [Fact]
    public async Task ApplyCombinedFirstNameAndAgeReturnEmpty()
    {
        var users = await GetUsers();

        QueryItem queryItem1 = new QueryItem(propertyName: "FirstName", propertyValue: "Giovanni");
        QueryItem queryItem2 = new QueryItem(propertyName: "Age", propertyValue: "19", propertyType: "int", operatorType: QueryOperator.Equal);

        var queryItemList = new List<QueryItem> { queryItem1, queryItem2 };

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItemList, "User"));

        var result = usersQuery.ToList();
        Assert.Empty(result);
    }

    [Fact]
    public async Task ApplyOperatorOnNestedObjectThatCanBeNull()
    {
        var users = await GetUsers();
        QueryItem queryItem1 = new QueryItem(propertyName: "Address.City", propertyValue: "Milano", parentCanBeNull: true);

        var queryItemList = new List<QueryItem> { queryItem1 };

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItemList, "User"));

        var result = usersQuery.ToList();
        Assert.Single(result);
    }

    [Fact]
    public async Task ApplyOperatorWithAgeBetweenTest()
    {
        var users = await GetUsers();

        QueryItem queryItem1 = new QueryItem(propertyName: "FirstName", propertyValue: "Giovanni or Giulio");
        QueryItem queryItem2 = new QueryItem(propertyName: "Age", propertyValue: "52", propertyType: "int", operatorType: QueryOperator.GreaterThan);
        QueryItem queryItem3 = new QueryItem(propertyName: "Age", propertyValue: "54", propertyType: "int", operatorType: QueryOperator.LessThan);

        var queryItemList = new List<QueryItem> { queryItem1, queryItem2, queryItem3 };

        IQueryable<User> usersQuery = users.AsQueryable();
        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItemList, "User"));

        var result = usersQuery.ToList();
        Assert.Single(result);
    }

    [Fact]
    public async Task ApplyAggregatorTest()
    {
        var users = await GetUsers();

        QueryItem queryItem1 = new QueryItem(propertyName: "FirstName", propertyValue: "Giovanni or Giulio");

        var queryItemList = new List<QueryItem> { queryItem1 };

        IQueryable<User> usersQuery = users.AsQueryable();

        usersQuery = usersQuery.Where(DynamicQueryBuilder.BuildAdvancedSearchExpressionTree<User>(queryItemList, "User"));

        int value = usersQuery.Sum(c => (int)c["Age"]);
        var result = usersQuery.ToList();

        Assert.Equal(72, value);
        Assert.Equal(2, result.Count);

    }
}
