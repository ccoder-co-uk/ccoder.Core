namespace cCoder.Core.Services
{
    /*    public class SecuredJoinService<TSecuredJoinEntity, THasRoles> : CoreService<TSecuredJoinEntity> 
            where TSecuredJoinEntity : class
            where THasRoles : class, IAmRoleSecured<TSecuredJoinEntity>
        {
            public SecuredJoinService(IAuthInfo auth, ICoreDataContext db) : base(auth, db) { }

            public override Task<TSecuredJoinEntity> AddAsync(TSecuredJoinEntity entity)
            {
                var leftSide = GetRelatedEntity<THasRoles>(entity, typeof(THasRoles).Name + "Id");
                var rightSide = GetRelatedEntity<Role>(entity, "RoleId");

                if (leftSide != null && rightSide != null && leftSide.UserCan(User, $"{typeof(TSecuredJoinEntity).Name}_create"))
                    return base.AddAsync(entity);
                else
                    throw new SecurityException("Access Denied!");
            }

            public override Task DeleteAsync(object key)
            {
                var entity = (TSecuredJoinEntity)key;
                var leftSide = GetRelatedEntity<THasRoles>(entity, typeof(THasRoles).Name + "Id");
                var rightSide = GetRelatedEntity<Role>(entity, "RoleId");

                if (leftSide != null && rightSide != null && leftSide.UserCan(User, $"{typeof(TSecuredJoinEntity).Name}_delete"))
                    return base.DeleteAsync(entity);
                else
                    throw new SecurityException("Access Denied!");
            }

            public TEntity GetRelatedEntity<TEntity>(TSecuredJoinEntity joinEntity, string joinKeyName) where TEntity : class
            {
                var keyValue = typeof(TSecuredJoinEntity).GetProperty(joinKeyName).GetValue(joinEntity);
                var parameter = Expression.Parameter(typeof(TEntity), "r");
                var idProperty = Expression.Property(parameter, typeof(TEntity).GetIdProperty().Name);
                var leftSideEqual = Expression.Equal(idProperty, Expression.Constant(keyValue));

                Expression<Func<TEntity, bool>> leftSideLambda = Expression.Lambda<Func<TEntity, bool>>(leftSideEqual, parameter);
                TEntity leftSideEntity = Db.GetAll<TEntity>(false).FirstOrDefault(leftSideLambda);
                return leftSideEntity;
            }
        }*/
}
