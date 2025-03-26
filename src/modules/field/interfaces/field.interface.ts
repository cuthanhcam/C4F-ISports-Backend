import { CreateFieldDto } from '../dto/create-field.dto';
import { UpdateFieldDto } from '../dto/update-field.dto';
import { Field } from '../entities/field.entity';

export interface IFieldService {
    create(createFieldDto: CreateFieldDto): Promise<Field>;
    findAll(query: any): Promise<{ fields: Field[]; total: number }>;
    findOne(id: number): Promise<Field>;
    update(id: number, updateFieldDto: UpdateFieldDto): Promise<Field>;
    remove(id: number): Promise<void>;
    search(query: any): Promise<{ fields: Field[]; total: number }>;
    getAvailableSlots(id: number, date: string): Promise<any[]>;
    getNearbyFields(query: any): Promise<{ fields: Field[]; total: number }>;
    uploadImages(id: number, files: Express.Multer.File[]): Promise<string[]>;
} 