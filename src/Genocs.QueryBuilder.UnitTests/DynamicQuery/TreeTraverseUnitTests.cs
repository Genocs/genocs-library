using Genocs.QueryBuilder.UnitTests.Models;
using Xunit;

namespace Genocs.QueryBuilder.UnitTests.DynamicQuery;

public class TreeTraverseUnitTests
{
    private async Task<TreeNode> BuildValidTreeLevel0()
    {
        TreeNode c01 = new TreeNode() { Name = "zero_first", Valid = true, ChildNodes = null };
        TreeNode c02 = new TreeNode() { Name = "zero_second", Valid = true, ChildNodes = null };

        List<TreeNode> childrenLevel0 = new List<TreeNode> { c01, c02 };

        TreeNode root = new TreeNode() { Name = "Root", Valid = true, ChildNodes = childrenLevel0 };

        return await Task.Run(() => root);
    }

    private async Task<TreeNode> BuildInvalidTreeLevel0()
    {
        TreeNode c01 = new TreeNode() { Name = "zero_first", Valid = true, ChildNodes = null };
        TreeNode c02 = new TreeNode() { Name = "zero_second", Valid = false, ChildNodes = null };

        List<TreeNode> childrenLevel0 = new List<TreeNode> { c01, c02 };

        TreeNode root = new TreeNode() { Name = "Root", Valid = true, ChildNodes = childrenLevel0 };

        return await Task.Run(() => root);
    }

    private async Task<TreeNode> BuildValidTreeLevel1()
    {
        TreeNode c01 = new TreeNode() { Name = "zero_first", Valid = true, ChildNodes = null };
        TreeNode c02 = new TreeNode() { Name = "zero_second", Valid = true, ChildNodes = null };

        TreeNode c11 = new TreeNode() { Name = "first", Valid = true, ChildNodes = null };
        TreeNode c12 = new TreeNode() { Name = "second", Valid = true, ChildNodes = null };
        TreeNode c13 = new TreeNode() { Name = "third", Valid = true, ChildNodes = null };
        TreeNode c14 = new TreeNode() { Name = "fourth", Valid = true, ChildNodes = null };
        TreeNode c15 = new TreeNode() { Name = "fifth", Valid = true, ChildNodes = null };

        TreeNode c21 = new TreeNode() { Name = "sixth", Valid = true, ChildNodes = null };
        TreeNode c22 = new TreeNode() { Name = "seventh", Valid = true, ChildNodes = null };
        TreeNode c23 = new TreeNode() { Name = "eighth", Valid = true, ChildNodes = null };
        TreeNode c24 = new TreeNode() { Name = "ninth", Valid = true, ChildNodes = null };
        TreeNode c25 = new TreeNode() { Name = "tenth", Valid = true, ChildNodes = null };
        TreeNode c26 = new TreeNode() { Name = "eleventh", Valid = true, ChildNodes = null };

        List<TreeNode> childrenLevel1 = new List<TreeNode> { c11, c12, c13, c14, c15 };
        List<TreeNode> childrenLevel2 = new List<TreeNode> { c21, c22, c23, c24, c25 };

        c01.ChildNodes = childrenLevel1;
        c02.ChildNodes = childrenLevel2;


        List<TreeNode> childrenLevel0 = new List<TreeNode> { c01, c02 };


        TreeNode root = new TreeNode() { Name = "Root", Valid = true, ChildNodes = childrenLevel0 };

        return await Task.Run(() => root);
    }

    private async Task<TreeNode> BuildInvalidTreeLevel1()
    {
        TreeNode c01 = new TreeNode() { Name = "zero_first", Valid = true, ChildNodes = null };
        TreeNode c02 = new TreeNode() { Name = "zero_second", Valid = true, ChildNodes = null };

        TreeNode c11 = new TreeNode() { Name = "first", Valid = true, ChildNodes = null };
        TreeNode c12 = new TreeNode() { Name = "second", Valid = true, ChildNodes = null };
        TreeNode c13 = new TreeNode() { Name = "third", Valid = true, ChildNodes = null };
        TreeNode c14 = new TreeNode() { Name = "fourth", Valid = false, ChildNodes = null };
        TreeNode c15 = new TreeNode() { Name = "fifth", Valid = true, ChildNodes = null };

        TreeNode c21 = new TreeNode() { Name = "sixth", Valid = true, ChildNodes = null };
        TreeNode c22 = new TreeNode() { Name = "seventh", Valid = true, ChildNodes = null };
        TreeNode c23 = new TreeNode() { Name = "eighth", Valid = true, ChildNodes = null };
        TreeNode c24 = new TreeNode() { Name = "ninth", Valid = true, ChildNodes = null };
        TreeNode c25 = new TreeNode() { Name = "tenth", Valid = true, ChildNodes = null };
        TreeNode c26 = new TreeNode() { Name = "eleventh", Valid = true, ChildNodes = null };

        List<TreeNode> childrenLevel1 = new List<TreeNode> { c11, c12, c13, c14, c15 };
        List<TreeNode> childrenLevel2 = new List<TreeNode> { c21, c22, c23, c24, c25 };

        c01.ChildNodes = childrenLevel1;
        c02.ChildNodes = childrenLevel2;


        List<TreeNode> childrenLevel0 = new List<TreeNode> { c01, c02 };


        TreeNode root = new TreeNode() { Name = "Root", Valid = true, ChildNodes = childrenLevel0 };

        return await Task.Run(() => root);
    }

    private async Task<TreeNode> BuildValidTreeLevel3()
    {
        TreeNode c01 = new TreeNode() { Name = "zero_first", Valid = true, ChildNodes = null };
        TreeNode c02 = new TreeNode() { Name = "zero_second", Valid = true, ChildNodes = null };

        TreeNode c11 = new TreeNode() { Name = "first", Valid = true, ChildNodes = null };
        TreeNode c12 = new TreeNode() { Name = "second", Valid = true, ChildNodes = null };
        TreeNode c13 = new TreeNode() { Name = "third", Valid = true, ChildNodes = null };
        TreeNode c14 = new TreeNode() { Name = "fourth", Valid = true, ChildNodes = null };
        TreeNode c15 = new TreeNode() { Name = "fifth", Valid = true, ChildNodes = null };
        TreeNode c16 = new TreeNode() { Name = "sixth", Valid = true, ChildNodes = null };
        TreeNode c17 = new TreeNode() { Name = "seventh", Valid = true, ChildNodes = null };
        TreeNode c18 = new TreeNode() { Name = "eighth", Valid = true, ChildNodes = null };
        TreeNode c19 = new TreeNode() { Name = "ninth", Valid = true, ChildNodes = null };
        TreeNode c20 = new TreeNode() { Name = "tenth", Valid = true, ChildNodes = null };

        List<TreeNode> childrenLevel1 = new List<TreeNode> { c11, c12 };
        List<TreeNode> childrenLevel2 = new List<TreeNode> { c13, c14 };
        List<TreeNode> childrenLevel3 = new List<TreeNode> { c15, c16 };
        List<TreeNode> childrenLevel4 = new List<TreeNode> { c17, c18 };
        List<TreeNode> childrenLevel5 = new List<TreeNode> { c19, c20 };


        c01.ChildNodes = childrenLevel1;
        c02.ChildNodes = childrenLevel2;

        c11.ChildNodes = childrenLevel3;
        c11.ChildNodes = childrenLevel4;

        c18.ChildNodes = childrenLevel5;

        List<TreeNode> childrenLevel0 = new List<TreeNode> { c01, c02 };


        TreeNode root = new TreeNode() { Name = "Root", Valid = true, ChildNodes = childrenLevel0 };

        return await Task.Run(() => root);
    }

    private async Task<TreeNode> BuildInvalidTreeLevel3()
    {
        TreeNode c01 = new TreeNode() { Name = "zero_first", Valid = true, ChildNodes = null };
        TreeNode c02 = new TreeNode() { Name = "zero_second", Valid = true, ChildNodes = null };

        TreeNode c11 = new TreeNode() { Name = "first", Valid = true, ChildNodes = null };
        TreeNode c12 = new TreeNode() { Name = "second", Valid = true, ChildNodes = null };
        TreeNode c13 = new TreeNode() { Name = "third", Valid = true, ChildNodes = null };
        TreeNode c14 = new TreeNode() { Name = "fourth", Valid = true, ChildNodes = null };
        TreeNode c15 = new TreeNode() { Name = "fifth", Valid = true, ChildNodes = null };
        TreeNode c16 = new TreeNode() { Name = "sixth", Valid = true, ChildNodes = null };
        TreeNode c17 = new TreeNode() { Name = "seventh", Valid = true, ChildNodes = null };
        TreeNode c18 = new TreeNode() { Name = "eighth", Valid = true, ChildNodes = null };
        TreeNode c19 = new TreeNode() { Name = "ninth", Valid = false, ChildNodes = null };
        TreeNode c20 = new TreeNode() { Name = "tenth", Valid = true, ChildNodes = null };

        List<TreeNode> childrenLevel1 = new List<TreeNode> { c11, c12 };
        List<TreeNode> childrenLevel2 = new List<TreeNode> { c13, c14 };
        List<TreeNode> childrenLevel3 = new List<TreeNode> { c15, c16 };
        List<TreeNode> childrenLevel4 = new List<TreeNode> { c17, c18 };
        List<TreeNode> childrenLevel5 = new List<TreeNode> { c19, c20 };


        c01.ChildNodes = childrenLevel1;
        c02.ChildNodes = childrenLevel2;

        c11.ChildNodes = childrenLevel3;
        c11.ChildNodes = childrenLevel4;

        c18.ChildNodes = childrenLevel5;

        List<TreeNode> childrenLevel0 = new List<TreeNode> { c01, c02 };


        TreeNode root = new TreeNode() { Name = "Root", Valid = true, ChildNodes = childrenLevel0 };

        return await Task.Run(() => root);
    }

    [Fact]
    public async Task TraverseValidLevel0TestAsync()
    {
        TreeNode tree = await BuildValidTreeLevel0();
        bool result = tree.FindDescendant();

        Assert.True(result);
    }

    [Fact]
    public async Task TraverseInvalidLevel0TestAsync()
    {
        TreeNode tree = await BuildInvalidTreeLevel0();
        bool result = tree.FindDescendant();

        Assert.False(result);
    }

    [Fact]
    public async Task TraverseValidLevel1TestAsync()
    {
        TreeNode tree = await BuildValidTreeLevel1();
        bool result = tree.FindDescendant();

        Assert.True(result);
    }

    [Fact]
    public async Task TraverseInvalidLevel1TestAsync()
    {
        TreeNode tree = await BuildInvalidTreeLevel1();
        bool result = tree.FindDescendant();

        Assert.False(result);
    }


    [Fact]
    public async Task TraverseValidLevel3TestAsync()
    {
        TreeNode tree = await BuildValidTreeLevel3();
        bool result = tree.FindDescendant();

        Assert.True(result);
    }

    [Fact]
    public async Task TraverseInvalidLevel3TestAsync()
    {
        TreeNode tree = await BuildInvalidTreeLevel3();
        bool result = tree.FindDescendant();

        Assert.False(result);
    }
}