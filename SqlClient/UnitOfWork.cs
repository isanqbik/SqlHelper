using Microsoft.Data.SqlClient;
using SqlClient.SeedWork;
using static System.Console;

namespace SqlClient;

public class UnitOfWork(IDatabase database) : IDisposable, IAsyncDisposable
{
    public void Work()
    {
        try
        {
            InternalWork();
        }
        catch (SqlException exception)
        {
            WriteLine("An error occurred while executing the database operation. {0}", exception);
        }
        catch (Exception e)
        {
            WriteLine("An error occurred while executing the operation. {0}", e);
            throw;
        }

        return;

        void InternalWork()
        {
            while (true)
            {
                WriteLine("Choose an operation: 1)Read All | 2)Insert Note | 3)Update Note | 4)Delete Note | 5)Massive insert | 6) Exit");
                var choice = ReadLine();

                switch (choice)
                {
                    case "1":
                        Read();
                        break;
                    case "2":
                        Insert();
                        break;
                    case "3":
                        Update();
                        break;
                    case "4":
                        Delete();
                        break;
                    case "5":
                        MassiveInsert();
                        break;
                    case "6":
                        return;
                    default:
                        WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }
    }

    private void Read()
    {
        foreach (var note in database.Read())
            WriteLine($"Id: {note.Id}, Note: {note.Note}, Expiring Date: {note.Expiring}");
    }

    private void Insert()
    {
        Write("Enter note: ");
        var note = ReadLine();

        if (string.IsNullOrEmpty(note))
        {
            WriteLine("Note cannot be noll or empty.");
            return;
        }

        database.Insert(note);
        WriteLine("Note inserted successfully.");
    }

    private void MassiveInsert()
    {
        Write("Enter notes separated by , or ; : ");
        var notes = ReadLine();
       
        database.MassiveInsert(notes?.Split([',',';'],StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? []);
        
        WriteLine("Notes inserted successfully.");
    }

    private void Update()
    {
        Write("Enter note Id to update: ");
        if (int.TryParse(ReadLine(), out var id))
        {
            Write("Enter new note: ");

            var note = ReadLine();
            if (string.IsNullOrEmpty(note))
            {
                WriteLine("Note cannot be noll or empty.");
                return;
            }

            database.Update(id, note);
            WriteLine("Note updated successfully.");
        }
        else
        {
            WriteLine("Invalid Id. Please enter a valid integer.");
        }
    }

    private void Delete()
    {
        Write("Enter note Id to delete: ");
        if (int.TryParse(ReadLine(), out var id))
        {
            Write("Are you sure you want to delete this note? (yes/no): ");
            var confirmation = ReadLine();

            if (confirmation?.ToLower() == "yes")
            {
                database.Delete(id);
                WriteLine("Note deleted successfully.");
            }
            else
            {
                WriteLine("Deletion cancelled.");
            }
        }
        else
        {
            WriteLine("Invalid Id. Please enter a valid integer.");
        }
    }

    public async ValueTask DisposeAsync() => await database.DisposeAsync();

    public void Dispose() => database.Dispose();
}