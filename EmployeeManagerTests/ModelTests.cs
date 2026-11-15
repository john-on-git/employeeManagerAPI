using EmployeeManager.Models.EF;
using EmployeeManager.Models.ViewModel;
using Moq;

namespace EmployeeManagerTests;


[TestFixture]
public class ModelTests
{
    [SetUp]
    public void Setup()
    {
    }
    
    [TestCase (1,"Alice", "These are Alice's notes.")]
    [TestCase (2,  "Bob", "These are Bob's notes")]
    public void EmployeeToViewModel(int id, string name, string notes)
    {
        var mockEmployee = new Mock<Employee>()
            .SetupProperty(x => x.Id, id)
            .SetupProperty(x => x.Name, name)
            .SetupProperty(x => x.Notes, notes)
            .Object;
        
        var viewModel = new EmployeeViewModel(mockEmployee);
        
        using (Assert.EnterMultipleScope())
        {
            // the properties were copied over
            Assert.That(viewModel.Id, Is.EqualTo(id));
            Assert.That(viewModel.Name, Is.EqualTo(name));
            Assert.That(viewModel.Notes, Is.EqualTo(notes));
        }
    }
    

    [TestCase (1,"user1","Alice", "These are Alice's notes.")]
    [TestCase (2,  "user2","Bob", "These are Bob's notes")]
    public void EmployeeFromViewModel(int id, string userId, string name, string notes)
    {
        var employee = new EmployeeViewModel(id, name, notes).ToEFModel(userId);
        
        using (Assert.EnterMultipleScope())
        {
            // the properties were copied over
            Assert.That(employee.Id, Is.EqualTo(id));
            Assert.That(employee.UserId, Is.EqualTo(userId));
            Assert.That(employee.Name, Is.EqualTo(name));
            Assert.That(employee.Notes, Is.EqualTo(notes));
            
            // these properties are set to the expected value
            Assert.That(employee.WorkItems, Is.Empty);
        }
    }

    [TestCase (1,"user1", "Boil Water", "Turn on the kettle to boil the water.", 3,"Alice", "Alice notes.", 4)]
    [TestCase (2,  "user2", "Place Teabag", "Place the teabag in the cup.", 4, "Bob", "Bob notes.", 5)]
    public void WorkItemToViewModel(int id, string userId, string title, string description, int assignedToId, string assignedToName, string assignedToNotes, int categoryId)
    {
        var mockEmployee = new Mock<Employee>()
            .SetupProperty(x => x.Id, assignedToId)
            .SetupProperty(x => x.Name, assignedToName)
            .SetupProperty(x => x.Notes, assignedToNotes)
            .Object;
        
        var mockWorkItem = new Mock<WorkItem>()
            .SetupProperty(x => x.Id, id)
            .SetupProperty(x => x.Title, title)
            .SetupProperty(x => x.Description, description)
            .SetupProperty(x => x.AssignedTo, mockEmployee)
            .SetupProperty(x => x.AssignedToId, assignedToId)
            .SetupProperty(x => x.CategoryId, categoryId)
            .Object;
        
        var viewModel = new WorkItemViewModel(mockWorkItem);
        
        using (Assert.EnterMultipleScope())
        {
            // the properties were copied over
            Assert.That(viewModel.Id, Is.EqualTo(id));
            Assert.That(viewModel.Title, Is.EqualTo(title));
            Assert.That(viewModel.Description, Is.EqualTo(description));
            Assert.That(viewModel.AssignedTo?.Id, Is.EqualTo(assignedToId));
            Assert.That(viewModel.AssignedTo?.Name, Is.EqualTo(assignedToName));
            Assert.That(viewModel.AssignedTo?.Notes, Is.EqualTo(assignedToNotes));
            Assert.That(viewModel.CategoryId, Is.EqualTo(categoryId));
        }
    }

    [TestCase (1,"user1", "Boil Water", "Turn on the kettle to boil the water.", 3,"Alice", "Alice notes.", 4)]
    [TestCase (2,  "user2", "Place Teabag", "Place the teabag in the cup.", 4, "Bob", "Bob notes.", 5)]
    public void WorkItemFromViewModel(int id, string userId, string title, string description, int assignedToId, string assignedToName, string assignedToNotes, int categoryId)
    {
        var mockEmployee = new Mock<Employee>()
            .SetupProperty(x => x.Id, assignedToId)
            .SetupProperty(x => x.Name, assignedToName)
            .SetupProperty(x => x.Notes, assignedToNotes)
            .Object;
        
        var workItem = new WorkItemViewModel(id, title, description, mockEmployee, categoryId).ToEFModel(userId);
        
        using (Assert.EnterMultipleScope())
        {
            // the properties were copied over
            Assert.That(workItem.Id, Is.EqualTo(id));
            Assert.That(workItem.UserId, Is.EqualTo(userId));
            Assert.That(workItem.Title, Is.EqualTo(title));
            Assert.That(workItem.Description, Is.EqualTo(description));
            Assert.That(workItem.AssignedToId, Is.EqualTo(assignedToId));
            Assert.That(workItem.CategoryId, Is.EqualTo(categoryId));
            
            // these properties are set to the expected value
            Assert.That(workItem.AssignedTo, Is.Null);
            Assert.That(workItem.Category, Is.Null);
        }
    }
    private static IEnumerable<(int, string, ICollection<WorkItem>)> YieldWorkItemCategoryToViewModelTestCases()
    {
        yield return (1, "Empty", []);
        yield return (2, "Just One", [MockItem("Apple"), MockItem("Banana"), MockItem("Cherry")]);
        yield return (3, "Many", [.."sdsandoasdaosdlskahdnasdasdhasdioajsldashdasiuodashdasldahsdaskjldashdsakldhasdl".Select(c => MockItem($"{c}"))]);
        yield break;

        WorkItem MockItem(string fruit)
        {
            return new Mock<WorkItem>()
                .SetupProperty(x => x.Id, 1)
                .SetupProperty(x => x.Title, $"Pick {fruit}")
                .SetupProperty(x => x.Description, $"Pick all the {fruit.ToLowerInvariant()}s.")
                .Object;
        }
    }
    
    [TestCaseSource(nameof(YieldWorkItemCategoryToViewModelTestCases))]
    public void WorkItemCategoryToViewModel((int, string, ICollection<WorkItem>) tup)
    {
        var (id, name, workItems) = tup;
        
        var mockWorkItemCategory = new Mock<WorkItemCategory>()
            .SetupProperty(x => x.Id, id)
            .SetupProperty(x => x.Name, name)
            .SetupProperty(x => x.WorkItems, workItems)
            .Object;
        
        var viewModel = new WorkItemCategoryViewModel(mockWorkItemCategory);
        
        using (Assert.EnterMultipleScope())
        {
            // the properties were copied over
            Assert.That(viewModel.Id, Is.EqualTo(id));
            Assert.That(viewModel.Name, Is.EqualTo(name));
            
            Assert.That(viewModel.WorkItems, Has.Count.EqualTo(workItems.Count));
            foreach(var (actual, expected) in Enumerable.Zip(viewModel.WorkItems, workItems))
            {
                // the properties were copied over
                Assert.That(actual.Id, Is.EqualTo(expected.Id));
                Assert.That(actual.Title, Is.EqualTo(expected.Title));
                Assert.That(actual.Description, Is.EqualTo(expected.Description));
            }
        }
    }

    private static IEnumerable<(int, string, string, ICollection<WorkItem>)> YieldWorkItemCategoryFromViewModelTestCases()
    {
        yield return (1, "user1", "Empty", []);
        yield return (2, "user2", "Just One", [MockItem("Apple"), MockItem("Banana"), MockItem("Cherry")]);
        yield return (3, "user3", "Many", [.."sdsandoasdaosdlskahdnasdasdhasdioajsldashdasiuodashdasldahsdaskjldashdsakldhasdl".Select(c => MockItem($"{c}"))]);
        yield break;

        WorkItem MockItem(string fruit)
        {
            return new Mock<WorkItem>()
                .SetupProperty(x => x.Id, 1)
                .SetupProperty(x => x.Title, $"Pick {fruit}")
                .SetupProperty(x => x.Description, $"Pick all the {fruit.ToLowerInvariant()}s.")
                .Object;
        }
    }
    
    [TestCaseSource(nameof(YieldWorkItemCategoryFromViewModelTestCases))]
    public void WorkItemCategoryFromViewModel((int id, string userId, string name, ICollection<WorkItem> workItems) testCase)
    {
        var category = new WorkItemCategoryViewModel(testCase.id, testCase.name, testCase.workItems).ToEFModel(testCase.userId);
        
        using (Assert.EnterMultipleScope())
        {
            // the properties were copied over
            Assert.That(category.Id, Is.EqualTo(testCase.id));
            Assert.That(category.UserId, Is.EqualTo(testCase.userId));
            Assert.That(category.Name, Is.EqualTo(testCase.name));
            foreach(var (actual, expected) in Enumerable.Zip(category.WorkItems, testCase.workItems))
            {
                // the properties were copied over
                Assert.That(actual.Id, Is.EqualTo(expected.Id));
                Assert.That(actual.Title, Is.EqualTo(expected.Title));
                Assert.That(actual.Description, Is.EqualTo(expected.Description));
            }
        }
    }


}