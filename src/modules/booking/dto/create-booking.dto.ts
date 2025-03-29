import { IsNotEmpty, IsDate, IsString, IsNumber, Min } from 'class-validator';

export class CreateBookingDto {
  @IsNotEmpty()
  @IsDate()
  startTime: Date;

  @IsNotEmpty()
  @IsDate()
  endTime: Date;

  @IsNotEmpty()
  @IsString()
  fieldId: string;

  @IsNotEmpty()
  @IsString()
  userId: string;

  @IsNotEmpty()
  @IsNumber()
  @Min(0)
  totalPrice: number;
} 