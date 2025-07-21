using System.ComponentModel.DataAnnotations;

namespace ShiftLogger.Presentation.Menu;

public enum MenuOption
{
    [Display(Name = "Add a worker to the shifts logger")]
    AddWorker,
    [Display(Name = "Update a worker that has been added to the shifts logger")]
    UpdateWorker,
    [Display(Name = "Delete a worker from the shifts logger")]
    DeleteWorker,
    [Display(Name = "Show detailed information on a worker by id")]
    ShowWorkerById,
    [Display(Name = "Show detailed information on a worker by name")]
    ShowWorkerByName,
    [Display(Name = "Show all workers")]
    ShowWorkers,
    [Display(Name = "Log a new shift")]
    CreateShift,
    [Display(Name = "Update an existing shift")]
    UpdateShift,
    [Display(Name = "Delete an existing shift")]
    DeleteShift,
    [Display(Name = "Show a shift based on id")]
    ShowShiftById,
    [Display(Name = "Show all shifts for a worker based on worker id")]
    ShowShiftsByWorkerId,
    [Display(Name = "Show all shifts")]
    ShowShifts,
    [Display(Name = "Exit the program")]
    Exit,
}

