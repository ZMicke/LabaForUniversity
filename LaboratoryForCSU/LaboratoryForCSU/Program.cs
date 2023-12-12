using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Введите математическое выражение:");
        var input = Console.ReadLine();

        var tokens = BreakInput(input);

        Console.WriteLine("Числа выражения:");
        Console.WriteLine(string.Join(" ", tokens.Item1));


        Console.WriteLine("Операции выражения:");
        Console.WriteLine(string.Join(" ", tokens.Item2.Select(o => o.Operation)));


        var result = SolveExpression(input);
        Console.WriteLine("Результат вычисления выражения: ");
        Console.WriteLine(result);

        var rpn = ConvertRPN(input);
        Console.WriteLine("ОПЗ: ");
        Console.WriteLine(string.Join(" ", rpn));

        var resultRPN = CalculateRPN(rpn);
        Console.WriteLine("Результат подсчета ОПЗ: ");
        Console.WriteLine(resultRPN);

    }

    public static (List<double>, List<OperationPriority>) BreakInput(string input)
    {
        var numbers = new List<double>();
        var operations = new List<OperationPriority>();
        var currentNumber = "";
        int newPriority = 0;


        for (int i = 0; i < input.Length; i++)
        {
            char element = input[i];

            if (char.IsDigit(element) || element == '.')
            {
                currentNumber += element;
            }
            else
            {

                if (!string.IsNullOrWhiteSpace(currentNumber))
                {
                    numbers.Add(double.Parse(currentNumber, CultureInfo.InvariantCulture));
                    currentNumber = "";
                }

                if (element == '(')
                {
                    newPriority++;
                }
                else if (element == ')')
                {
                    newPriority--;
                }
                else if ("+-*/".Contains(element))
                {
                    int priority = (element == '*' || element == '/') ? newPriority + 1 : newPriority;
                    operations.Add(new OperationPriority { Operation = element, Priority = priority });
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(currentNumber))
        {
            numbers.Add(double.Parse(currentNumber, CultureInfo.InvariantCulture));
        }

        return (numbers, operations);
    }

    public static double SolveExpression(string expression)
    {
        Stack<double> key = new Stack<double>();
        Stack<char> action = new Stack<char>();

        for (int i = 0; i < expression.Length; i++)
        {
            if (expression[i] == ' ')
                continue;

            if (expression[i] == '(')
            {
                action.Push(expression[i]);
            }
            else if (char.IsDigit(expression[i]))
            {
                string zz = "";
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                    zz += expression[i++];

                key.Push(double.Parse(zz, CultureInfo.InvariantCulture));
                i--;
            }
            else if (expression[i] == ')')
            {
                while (action.Peek() != '(')
                    key.Push(Make(action.Pop(), key.Pop(), key.Pop()));
                action.Pop();
            }
            else if ("+-*/".Contains(expression[i]))
            {
                while (action.Count != 0 && CheckPriority(expression[i], action.Peek()))
                    key.Push(Make(action.Pop(), key.Pop(), key.Pop()));

                action.Push(expression[i]);
            }
        }

        while (action.Count != 0)
            key.Push(Make(action.Pop(), key.Pop(), key.Pop()));

        return key.Pop();
    }

    public static bool CheckPriority(char operation1, char operation2)
    {
        if (operation2 == '(' || operation2 == ')')
            return false;
        if ((operation1 == '*' || operation1 == '/') && (operation2 == '+' || operation2 == '-'))
            return false;
        else
            return true;
    }

    public static double Make(char operation, double number1, double number2)
    {
        switch (operation)
        {
            case '+': return number2 + number1;
            case '-': return number2 - number1;
            case '*': return number2 * number1;
            case '/':
                if (number1 == 0)
                    throw new DivideByZeroException("Делить на ноль нельзя!");
                return number2 / number1;
        }
        return 0;
    }
    // Метод преобразования в ОПЗ
    public static List<object> ConvertRPN(string expression)
    {
        List<object> resultExpression = new List<object>();
        Stack<char> operation = new Stack<char>();

        for (int i = 0; i < expression.Length; i++)
        {
            if (char.IsDigit(expression[i]) || expression[i] == '.')
            {
                var number = new StringBuilder();
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                    number.Append(expression[i++]);
                resultExpression.Add(number.ToString());
                i--;
            }
            else if (expression[i] == '(')
            {
                operation.Push(expression[i]);
            }
            else if ("+-*/".Contains(expression[i]))
            {
                while (operation.Count != 0 && Priority(operation.Peek()) >= Priority(expression[i]))
                    resultExpression.Add(operation.Pop());
                operation.Push(expression[i]);
            }
            else if (expression[i] == ')')
            {
                while (operation.Count > 0 && operation.Peek() != '(')
                    resultExpression.Add(operation.Pop());
                if (operation.Count > 0 && operation.Peek() == '(')
                    operation.Pop();
            }
        }

        while (operation.Count != 0)
            resultExpression.Add(operation.Pop());

        return resultExpression;
    }
    public static double CalculateRPN(List<object> resultExpression)
    {
        Stack<double> value = new Stack<double>();

        foreach (var tok in resultExpression)
        {
            if (tok is string str && double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
            {
                value.Push(v);
            }
            else if (tok is char p)
            {
                if (value.Count < 2)
                    throw new InvalidOperationException("Недостаточно данных для выполнения операции!");
                value.Push(AppOperation(p, value.Pop(), value.Pop()));
            }
            else
            {
                throw new InvalidOperationException("Недопустимый элемент в выражении: " + tok);
            }
        }
        return value.Pop();
    }
    //Приоритеты
    public static int Priority(char p)
    {
        if (p == '*' || p == '/')
            return 2;
        if (p == '+' || p == '-')
            return 1;
        return 0;
    }
    public static double AppOperation(char p, double b, double a)
    {
        switch (p)
        {
            case '+': return a + b;
            case '-': return a - b;
            case '*': return a * b;
            case '/':
                if (b == 0)
                    throw new DivideByZeroException("Попытка деления на ноль!");
                return a / b;
        }
        return 0;
    }
}

struct OperationPriority
{
    public char Operation { get; set; }
    public int Priority { get; set; }
}
