using System;
using System.Collections.Generic;

namespace Riada.Domain;

public abstract class Person
{
    private readonly int _id;
    private readonly string _firstName;
    private readonly string _lastName;
    private readonly string _email;

    protected Person(int id, string firstName, string lastName, string email)
    {
        _id = id;
        _firstName = firstName;
        _lastName = lastName;
        _email = email;
    }

    public int Id => _id;
    public string FirstName => _firstName;
    public string LastName => _lastName;
    public string Email => _email;

    public virtual decimal CalculateEngagementScore()
    {
        throw new NotImplementedException();
    }

    public virtual string DescribeRole()
    {
        throw new NotImplementedException();
    }
}

public sealed class Member : Person
{
    private string _status;
    private readonly DateOnly _dateOfBirth;
    private readonly List<Contract> _contracts;

    public Member(
        int id,
        string firstName,
        string lastName,
        string email,
        string status,
        DateOnly dateOfBirth,
        IEnumerable<Contract>? contracts = null) : base(id, firstName, lastName, email)
    {
        _status = status;
        _dateOfBirth = dateOfBirth;
        _contracts = contracts is null ? [] : new List<Contract>(contracts);
    }

    public string Status => _status;
    public DateOnly DateOfBirth => _dateOfBirth;
    public IReadOnlyCollection<Contract> Contracts => _contracts.AsReadOnly();

    public override decimal CalculateEngagementScore()
    {
        throw new NotImplementedException();
    }

    public override string DescribeRole()
    {
        throw new NotImplementedException();
    }

    public Contract CreateContract(SubscriptionPlan plan, Club club, DateOnly startDate)
    {
        throw new NotImplementedException();
    }
}

public sealed class Employee : Person
{
    private Club _club;
    private readonly string _role;

    public Employee(
        int id,
        string firstName,
        string lastName,
        string email,
        Club club,
        string role) : base(id, firstName, lastName, email)
    {
        _club = club;
        _role = role;
    }

    public Club Club => _club;
    public string Role => _role;

    public override decimal CalculateEngagementScore()
    {
        throw new NotImplementedException();
    }

    public override string DescribeRole()
    {
        throw new NotImplementedException();
    }
}

public sealed class Club
{
    private readonly int _id;
    private readonly string _name;
    private readonly string _addressCity;
    private readonly List<Employee> _employees;
    private readonly List<Contract> _contracts;

    public Club(int id, string name, string addressCity, IEnumerable<Employee>? employees = null, IEnumerable<Contract>? contracts = null)
    {
        _id = id;
        _name = name;
        _addressCity = addressCity;
        _employees = employees is null ? [] : new List<Employee>(employees);
        _contracts = contracts is null ? [] : new List<Contract>(contracts);
    }

    public int Id => _id;
    public string Name => _name;
    public string AddressCity => _addressCity;
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();
    public IReadOnlyCollection<Contract> Contracts => _contracts.AsReadOnly();

    public void AssignEmployee(Employee employee)
    {
        throw new NotImplementedException();
    }
}

public sealed class SubscriptionPlan
{
    private readonly int _id;
    private readonly string _planName;
    private readonly decimal _basePrice;
    private readonly int _commitmentMonths;

    public SubscriptionPlan(int id, string planName, decimal basePrice, int commitmentMonths)
    {
        _id = id;
        _planName = planName;
        _basePrice = basePrice;
        _commitmentMonths = commitmentMonths;
    }

    public int Id => _id;
    public string PlanName => _planName;
    public decimal BasePrice => _basePrice;
    public int CommitmentMonths => _commitmentMonths;

    public decimal CalculateMonthlyCost()
    {
        throw new NotImplementedException();
    }
}

public sealed class Contract
{
    private readonly int _id;
    private readonly Member _member;
    private readonly SubscriptionPlan _plan;
    private readonly Club _homeClub;
    private readonly DateOnly _startDate;
    private readonly List<Invoice> _invoices;

    public Contract(
        int id,
        Member member,
        SubscriptionPlan plan,
        Club homeClub,
        DateOnly startDate,
        IEnumerable<Invoice>? invoices = null)
    {
        _id = id;
        _member = member;
        _plan = plan;
        _homeClub = homeClub;
        _startDate = startDate;
        _invoices = invoices is null ? [] : new List<Invoice>(invoices);
    }

    public int Id => _id;
    public Member Member => _member;
    public SubscriptionPlan Plan => _plan;
    public Club HomeClub => _homeClub;
    public DateOnly StartDate => _startDate;
    public IReadOnlyCollection<Invoice> Invoices => _invoices.AsReadOnly();

    public Invoice GenerateInvoice(DateOnly issuedOn, DateOnly dueDate)
    {
        throw new NotImplementedException();
    }

    public void Freeze(DateOnly fromDate, DateOnly toDate)
    {
        throw new NotImplementedException();
    }
}

public sealed class Invoice
{
    private readonly int _id;
    private readonly string _invoiceNumber;
    private readonly Contract _contract;
    private readonly decimal _amountInclTax;
    private readonly List<Payment> _payments;

    public Invoice(
        int id,
        string invoiceNumber,
        Contract contract,
        decimal amountInclTax,
        IEnumerable<Payment>? payments = null)
    {
        _id = id;
        _invoiceNumber = invoiceNumber;
        _contract = contract;
        _amountInclTax = amountInclTax;
        _payments = payments is null ? [] : new List<Payment>(payments);
    }

    public int Id => _id;
    public string InvoiceNumber => _invoiceNumber;
    public Contract Contract => _contract;
    public decimal AmountInclTax => _amountInclTax;
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    public decimal CalculateOutstandingAmount()
    {
        throw new NotImplementedException();
    }
}

public sealed class Payment
{
    private readonly int _id;
    private readonly Invoice _invoice;
    private readonly decimal _amount;
    private readonly DateTime _paidAt;
    private string _status;

    public Payment(int id, Invoice invoice, decimal amount, DateTime paidAt, string status)
    {
        _id = id;
        _invoice = invoice;
        _amount = amount;
        _paidAt = paidAt;
        _status = status;
    }

    public int Id => _id;
    public Invoice Invoice => _invoice;
    public decimal Amount => _amount;
    public DateTime PaidAt => _paidAt;
    public string Status => _status;

    public void MarkAsSucceeded()
    {
        throw new NotImplementedException();
    }
}
