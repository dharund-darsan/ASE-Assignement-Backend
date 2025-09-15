namespace quickBook.Dtos
{
    public class ExistenceCheckResultDto
    {
        public bool OrganizerExists { get; set; }
        public bool StatusExists { get; set; }
        public bool ParticipantsExist { get; set; }

        public bool AllExist => OrganizerExists && StatusExists && ParticipantsExist;
    }
}