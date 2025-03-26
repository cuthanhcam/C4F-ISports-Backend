import { IsString, IsNumber, IsOptional, IsEnum, IsArray, IsBoolean } from 'class-validator';
import { FieldStatus } from '../enums/field-status.enum';

export class CreateFieldDto {
    @IsString()
    name: string;

    @IsString()
    description: string;

    @IsString()
    address: string;

    @IsNumber()
    sportId: number;

    @IsNumber()
    latitude: number;

    @IsNumber()
    longitude: number;

    @IsEnum(FieldStatus)
    @IsOptional()
    status?: FieldStatus;

    @IsArray()
    @IsString({ each: true })
    @IsOptional()
    amenities?: string[];

    @IsBoolean()
    @IsOptional()
    isActive?: boolean;

    @IsNumber()
    @IsOptional()
    pricePerHour?: number;
} 