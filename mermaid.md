# Modelo de Domínio do Volts

O diagrama abaixo representa as principais entidades do sistema Volts e seus relacionamentos.

O código do diagrama pode ser visto por aqui:

```
erDiagram
    User {
        string Id PK
        string Name
        string Email
        string Password
    }
    
    Organization {
        string Id PK
        string Name
        string Description
    }
    
    OrganizationMember {
        string Id PK
        string UserId FK
        string OrganizationId FK
        enum Role
        datetime JoinedAt
        string InvitedById
    }
    
    Group {
        string Id PK
        string Name
        string Description
        string OrganizationId FK
    }
    
    Position {
        string Id PK
        string Name
        string Description
        string GroupId FK
    }
    
    Shift {
        string Id PK
        string Name
        string Description
        string Location
        datetime StartDateTime
        datetime EndDateTime
        string GroupId FK
        enum Status
    }
    
    ShiftPosition {
        string Id PK
        string ShiftId FK
        string PositionId FK
        int RequiredCount
        int VolunteersCount
    }
    
    ShiftPositionAssignment {
        string Id PK
        string UserId FK
        string ShiftPositionId FK
        enum Status
        string Notes
        datetime AppliedAt
        datetime ConfirmedAt
        datetime RejectedAt
    }
    
    %% Relationships
    User ||--o{ OrganizationMember : "pertence"
    Organization ||--o{ OrganizationMember : "tem"
    Organization ||--o{ Group : "tem"
    Group ||--o{ Position : "tem"
    Group ||--o{ Shift : "organiza"
    Shift ||--o{ ShiftPosition : "tem"
    Position ||--o{ ShiftPosition : "associada"
    ShiftPosition ||--o{ ShiftPositionAssignment : "tem"
    User ||--o{ ShiftPositionAssignment : "se inscreve"
```
