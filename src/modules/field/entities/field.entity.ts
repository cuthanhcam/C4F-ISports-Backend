import { Entity, PrimaryGeneratedColumn, Column, CreateDateColumn, UpdateDateColumn, ManyToOne, OneToMany } from 'typeorm';
import { User } from '../../user/entities/user.entity';
import { Sport } from '../../sport/entities/sport.entity';
import { Review } from '../../review/entities/review.entity';
import { Booking } from '../../booking/entities/booking.entity';
import { FieldStatus } from '../enums/field-status.enum';

@Entity('fields')
export class Field {
    @PrimaryGeneratedColumn()
    id: number;

    @Column()
    name: string;

    @Column('text')
    description: string;

    @Column()
    address: string;

    @Column('decimal', { precision: 10, scale: 8 })
    latitude: number;

    @Column('decimal', { precision: 11, scale: 8 })
    longitude: number;

    @Column({
        type: 'enum',
        enum: FieldStatus,
        default: FieldStatus.PENDING
    })
    status: FieldStatus;

    @Column('simple-array', { nullable: true })
    amenities: string[];

    @Column({ default: true })
    isActive: boolean;

    @Column('decimal', { precision: 10, scale: 2, nullable: true })
    pricePerHour: number;

    @Column('simple-array', { nullable: true })
    images: string[];

    @ManyToOne(() => User, user => user.fields)
    owner: User;

    @ManyToOne(() => Sport, sport => sport.fields)
    sport: Sport;

    @OneToMany(() => Review, review => review.field)
    reviews: Review[];

    @OneToMany(() => Booking, booking => booking.field)
    bookings: Booking[];

    @CreateDateColumn()
    createdAt: Date;

    @UpdateDateColumn()
    updatedAt: Date;
} 