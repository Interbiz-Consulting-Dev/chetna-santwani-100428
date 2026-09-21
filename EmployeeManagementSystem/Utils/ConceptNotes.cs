namespace EmployeeManagementSystem.Utils
{
    // This method exists solely to demonstrate boxing/unboxing as required by
    // the project spec — there is no organic use case for it in this app's
    // real functionality. (ArrayList.Add(staffId) also boxes, but that is an
    // accident of a legacy API, not a reason to teach the mechanism.)
    public static class ConceptNotes
    {
        public static void BoxingUnboxingExample()
        {
            int staffId = 17;
            object boxed = staffId;
            int unboxed = (int)boxed;
            Console.WriteLine($"Boxed then unboxed staff id: {staffId} → object → {unboxed}");
        }
    }
}
