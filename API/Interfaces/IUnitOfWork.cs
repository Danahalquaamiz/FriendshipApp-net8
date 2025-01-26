namespace API.Interfaces;

public interface IUnitOfWork

{
    IUserRepository UserRepository {get;}
    IMessageRepository MessageRepository {get;}
    ILikesRepository LikesRepository {get;}
    Task<bool> Complete(); //instead to save changes in every repository, we will commit changes in one place only. and the changes in all places must be successfull or the changes will not be updated.
    bool HasChanges();
}
