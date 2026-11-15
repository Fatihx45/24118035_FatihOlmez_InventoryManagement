namespace InventoryManagement.Interfaces
{
    /// <summary>
    /// Sistemde yapılan işlemlerin denetim loguna yazılabilmesi için arayüz.
    /// OOP İlkesi: Soyutlama (Abstraction)
    /// </summary>
    public interface ILoglanabilir
    {
        string LogFormatiOlustur();
    }
}
